import { test, expect } from '@playwright/test';

const PASS = 'test1234';

function moneyStat(page: import('@playwright/test').Page) {
  return page.locator('.titlebar-stat .v').filter({ hasText: /^\d+ g$/ });
}

async function registerAndEnter(page: import('@playwright/test').Page) {
  const user = `e2e_${Date.now().toString(36)}`;
  await page.goto('/register');
  await page.fill('input[name="username"]', user);
  await page.fill('input[name="password"]', PASS);
  await page.fill('input[name="confirmPassword"]', PASS);
  await Promise.all([
    page.waitForURL((url) => !url.pathname.includes('/register'), { timeout: 30_000 }),
    page.click('button[type="submit"]'),
  ]);
  if (page.url().includes('/login')) {
    await page.fill('input[name="username"]', user);
    await page.fill('input[name="password"]', PASS);
    await Promise.all([
      page.waitForURL('/', { timeout: 30_000 }),
      page.click('button[type="submit"]'),
    ]);
  }
  await page.waitForSelector('.geek-root', { timeout: 30_000 });
  await page.waitForTimeout(3_000);
  return user;
}

async function clickTab(page: import('@playwright/test').Page, label: string) {
  await page.locator('button.g-tab').filter({ hasText: label }).click();
  await page.waitForTimeout(1_500);
}

test.describe('CYBERPET OS', () => {
  test('注册登录并加载主界面', async ({ page }) => {
    await registerAndEnter(page);
    await expect(page.locator('.titlebar-title')).toContainText('CYBERPET OS');
    await expect(moneyStat(page)).toBeVisible();
  });

  test('钓鱼页装备属性正确渲染', async ({ page }) => {
    await registerAndEnter(page);
    await clickTab(page, '钓鱼');
    const content = await page.locator('.content-scroll').innerText();
    expect(content).not.toContain('@Session.Loadout');
    expect(content).toMatch(/抛投\d+/);
  });

  test('状态栏 tab 标签显示中文', async ({ page }) => {
    await registerAndEnter(page);
    await clickTab(page, '打工');
    await expect(page.locator('.statusbar')).toContainText('tab: 打工');
  });

  test('无猫粮时显示购买提示', async ({ page }) => {
    await registerAndEnter(page);
    await expect(page.locator('.care-no-food-hint')).toContainText('生活商店');
    await expect(page.locator('button:has-text("吞拿罐")')).toBeDisabled();
  });

  test('核心玩法：打工、钓鱼、商店购买', async ({ page }) => {
    test.setTimeout(60_000);
    await registerAndEnter(page);

    await clickTab(page, '打工');
    await page.click('button:has-text("exec(work)")');
    await page.waitForTimeout(2_000);
    await expect(page.locator('.titlebar-pill').filter({ hasText: 'WORKING' })).toBeVisible();

    await clickTab(page, '钓鱼');
    const startFish = page.locator('button').filter({ hasText: /StartIdleFishing/ }).first();
    if (await startFish.isVisible()) {
      await startFish.click();
      await page.waitForTimeout(2_000);
      await expect(page.locator('.titlebar-pill').filter({ hasText: 'FISHING' })).toBeVisible();
    }

    await clickTab(page, '生活商店');
    const moneyBefore = await moneyStat(page).innerText();
    await page.locator('button').filter({ hasText: /buy\(\d+ g\)/ }).first().click();
    await page.waitForTimeout(2_000);
    const moneyAfter = await moneyStat(page).innerText();
    expect(moneyBefore).not.toEqual(moneyAfter);
  });

  test('所有主标签页可切换', async ({ page }) => {
    await registerAndEnter(page);
    const tabs = ['家园', '钓鱼', '打工', '派遣', '鱼市', '装备', '烹饪', '炼金', '生活商店', '里程碑', '背包'];
    for (const tab of tabs) {
      await clickTab(page, tab);
      await expect(page.locator('.content-scroll')).toBeVisible();
    }
  });

  test('自动喂食器与水槽：一键批量装填、精力补充液兼容与自动装填', async ({ page }) => {
    test.setTimeout(60_000);
    await registerAndEnter(page);

    // 1. 在生活商店购买材料
    await clickTab(page, '生活商店');
    // 购买“超能红牛”精力补充液 (45g)
    await page.locator('button').filter({ hasText: /buy\(45 g\)/ }).first().click();
    await page.waitForTimeout(500);
    // 购买“猫乐滋”混合肉干粮 (15g)
    await page.locator('button').filter({ hasText: /buy\(15 g\)/ }).first().click();
    await page.waitForTimeout(500);
    // 购买凉白开水 (2g)
    await page.locator('button').filter({ hasText: /buy\(2 g\)/ }).first().click();
    await page.waitForTimeout(500);

    // 2. 解锁房间和家具
    await clickTab(page, '家园');

    // 解锁厨房 (200g)
    const kitchenLockBtn = page.locator('button.floor-room:has-text("厨房")');
    await kitchenLockBtn.click();
    await page.waitForTimeout(1000);
    await page.locator('button:has-text("unlock(200 g)")').click();
    await page.waitForTimeout(1000);

    // 回到客厅，购买饮水泉 (200g)
    await page.locator('button.floor-room:has-text("客厅")').click();
    await page.waitForTimeout(1000);
    await page.locator('button:has-text("buy(200 g)")').click();
    await page.waitForTimeout(1000);

    // 去厨房，购买智能喂食站 (400g)
    await page.locator('button.floor-room:has-text("厨房")').click();
    await page.waitForTimeout(1000);
    await page.locator('button:has-text("buy(400 g)")').click();
    await page.waitForTimeout(2000); // 等待 tick 运行

    // 验证自动装填未解锁时，槽位保持为空 (LOCKED tag exists)
    await expect(page.locator('.tag').filter({ hasText: 'LOCKED' })).toBeVisible();
    await expect(page.locator('.auto-care-slot.filled')).toHaveCount(0);

    // 测试手动一键批量装填食物
    const batchFillFeederBtn = page.locator('button:has-text("一键批量装填")').first();
    await expect(batchFillFeederBtn).toBeEnabled();
    await batchFillFeederBtn.click();
    await page.waitForTimeout(1000);

    // 验证食物和饮料被成功装填
    await expect(page.locator('.auto-care-slot.filled').filter({ hasText: '“超能红牛”精力补充液' })).toBeVisible();
    await expect(page.locator('.auto-care-slot.filled').filter({ hasText: '“猫乐滋”混合肉干粮' })).toBeVisible();

    // 回到客厅，测试手动一键批量装填水
    await page.locator('button.floor-room:has-text("客厅")').click();
    await page.waitForTimeout(1000);
    const batchFillWatererBtn = page.locator('button:has-text("一键批量装填")').first();
    await expect(batchFillWatererBtn).toBeEnabled();
    await batchFillWatererBtn.click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.auto-care-slot.filled').filter({ hasText: '凉白开水' })).toBeVisible();

    // 3. 测试解锁自动装填
    // 解锁自动装填系统 (1000g)
    const unlockAutoRefillBtn = page.locator('button:has-text("解锁(1000g)")').first();
    await expect(unlockAutoRefillBtn).toBeEnabled();
    await unlockAutoRefillBtn.click();
    await page.waitForTimeout(1000);
    await expect(page.locator('.tag').filter({ hasText: 'ACTIVE' })).toBeVisible();

    // 4. 验证解锁后自动装填生效
    // 去生活商店再买一个精力补充液
    await clickTab(page, '生活商店');
    await page.locator('button').filter({ hasText: /buy\(45 g\)/ }).first().click();
    await page.waitForTimeout(500);

    await clickTab(page, '家园');
    // 去厨房验证喂食器中自动装填了第二个精力补充液
    await page.locator('button.floor-room:has-text("厨房")').click();
    await page.waitForTimeout(2000);
    await expect(page.locator('.auto-care-slot.filled').filter({ hasText: '“超能红牛”精力补充液' })).toHaveCount(2);
  });
});
