using Microsoft.AspNetCore.Components;

namespace CyberPetApp.Components.Home;

/// <summary>Home Tab 子组件基类：非活动 Tab 不渲染；活动 Tab 仅在 TickGeneration 变化或子类 RequestRender 时刷新。
/// HomeHouseTab 不继承此类：本地房间选中态会被门控吞掉，改由 HouseFloorView 自持选中与详情面板。</summary>
public abstract class HomeTabBase : ComponentBase
{
    [Parameter] public bool IsActive { get; set; }
    [Parameter] public int TickGeneration { get; set; }

    private int _lastTick = -1;
    private bool _force;

    protected void RequestRender()
    {
        _force = true;
        StateHasChanged();
    }

    protected override bool ShouldRender()
    {
        if (!IsActive) return false;
        if (_force) return true;
        if (TickGeneration != _lastTick) return true;
        return ShouldRenderWithoutTick();
    }

    /// <summary>Tick 未变时是否仍需刷新（如钓鱼阶段动画）。</summary>
    protected virtual bool ShouldRenderWithoutTick() => false;

    protected override void OnAfterRender(bool firstRender)
    {
        _lastTick = TickGeneration;
        _force = false;
    }
}
