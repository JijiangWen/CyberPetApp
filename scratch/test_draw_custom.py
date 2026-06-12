import os
from PIL import Image, ImageDraw

OL = (18, 22, 34, 255)
SL = (140, 155, 175, 255)
SL_D = (90, 105, 125, 255)
SL_L = (200, 210, 225, 255)
WH = (255, 252, 245, 255)
CY = (0, 229, 200, 255)
CY_L = (120, 255, 235, 255)
PK = (244, 114, 182, 255)
PK_D = (200, 70, 140, 255)
PK_L = (255, 180, 210, 255)

def draw_rect(img, x, y, w, h, fill, outline=OL):
    draw = ImageDraw.Draw(img)
    if outline and w > 0 and h > 0:
        draw.rectangle([x - 1, y - 1, x + w, y + h], fill=outline)
    draw.rectangle([x, y, x + w - 1, y + h - 1], fill=fill)

def draw_crab():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    # Body shell
    draw_rect(img, 22, 26, 20, 12, SL)
    # Highlight on body
    draw_rect(img, 24, 28, 6, 2, SL_L, outline=None)
    
    # Eyes
    draw_rect(img, 26, 21, 2, 5, SL) # Left stalk
    draw_rect(img, 26, 19, 2, 2, OL, outline=None) # Left eye
    draw_rect(img, 27, 20, 1, 1, WH, outline=None)
    
    draw_rect(img, 36, 21, 2, 5, SL) # Right stalk
    draw_rect(img, 36, 19, 2, 2, OL, outline=None) # Right eye
    draw_rect(img, 37, 20, 1, 1, WH, outline=None)
    
    # Claws (arms and pinchers)
    draw_rect(img, 16, 24, 6, 4, SL) # Left arm
    draw_rect(img, 12, 17, 6, 7, SL) # Left claw base
    draw_rect(img, 10, 14, 3, 5, SL_D) # Left pincher 1
    draw_rect(img, 15, 14, 3, 5, SL_D) # Left pincher 2
    
    draw_rect(img, 42, 24, 6, 4, SL) # Right arm
    draw_rect(img, 46, 17, 6, 7, SL) # Right claw base
    draw_rect(img, 46, 14, 3, 5, SL_D) # Right pincher 1
    draw_rect(img, 51, 14, 3, 5, SL_D) # Right pincher 2
    
    # Legs (3 on each side)
    # Left leg 1
    draw_rect(img, 16, 32, 6, 3, SL_D)
    draw_rect(img, 12, 34, 4, 4, SL_D)
    # Left leg 2
    draw_rect(img, 18, 36, 4, 3, SL_D)
    draw_rect(img, 14, 38, 4, 4, SL_D)
    # Left leg 3
    draw_rect(img, 20, 40, 4, 3, SL_D)
    draw_rect(img, 17, 42, 3, 4, SL_D)
    
    # Right leg 1
    draw_rect(img, 42, 32, 6, 3, SL_D)
    draw_rect(img, 48, 34, 4, 4, SL_D)
    # Right leg 2
    draw_rect(img, 42, 36, 4, 3, SL_D)
    draw_rect(img, 46, 38, 4, 4, SL_D)
    # Right leg 3
    draw_rect(img, 40, 40, 4, 3, SL_D)
    draw_rect(img, 44, 42, 3, 4, SL_D)
    
    return img

def draw_turtle():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    # Head
    draw_rect(img, 29, 14, 6, 8, SL)
    # Eyes on head
    draw_rect(img, 28, 16, 2, 2, OL, outline=None)
    draw_rect(img, 34, 16, 2, 2, OL, outline=None)
    
    # Shell (oval shape)
    draw_rect(img, 20, 22, 24, 20, SL_D)
    # Inner shell pattern
    draw_rect(img, 24, 25, 16, 14, SL, outline=OL)
    draw_rect(img, 28, 29, 8, 6, SL_L, outline=OL)
    
    # Flippers/Legs
    # Front-left
    draw_rect(img, 14, 20, 6, 6, SL)
    draw_rect(img, 11, 23, 4, 5, SL)
    # Front-right
    draw_rect(img, 44, 20, 6, 6, SL)
    draw_rect(img, 49, 23, 4, 5, SL)
    # Back-left
    draw_rect(img, 16, 38, 5, 5, SL)
    draw_rect(img, 13, 41, 4, 4, SL)
    # Back-right
    draw_rect(img, 43, 38, 5, 5, SL)
    draw_rect(img, 47, 41, 4, 4, SL)
    
    # Tail
    draw_rect(img, 31, 42, 2, 5, SL)
    
    return img

def draw_jellyfish():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    # Bell (dome)
    draw_rect(img, 20, 18, 24, 12, SL)
    # Rounded top corners of dome
    draw_rect(img, 22, 16, 20, 2, SL)
    draw_rect(img, 24, 14, 16, 2, SL)
    # Highlight
    draw_rect(img, 26, 16, 6, 2, SL_L, outline=None)
    
    # Bell bottom ruffle/trim
    draw_rect(img, 18, 28, 28, 3, SL_D)
    
    # Tentacles dangling down
    # Tentacle 1 (left)
    draw_rect(img, 22, 31, 2, 12, SL_L)
    draw_rect(img, 20, 41, 2, 8, SL_L)
    # Tentacle 2 (mid-left)
    draw_rect(img, 27, 31, 2, 18, SL_D)
    draw_rect(img, 25, 47, 2, 8, SL_D)
    # Tentacle 3 (mid-right)
    draw_rect(img, 35, 31, 2, 20, SL_D)
    draw_rect(img, 37, 49, 2, 6, SL_D)
    # Tentacle 4 (right)
    draw_rect(img, 40, 31, 2, 14, SL_L)
    draw_rect(img, 42, 43, 2, 8, SL_L)
    
    return img

def draw_tadpole():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    # Body (round shape on the right side, swimming left)
    draw_rect(img, 28, 22, 16, 16, SL)
    # Eyes
    draw_rect(img, 32, 20, 3, 3, OL, outline=None)
    draw_rect(img, 33, 21, 1, 1, WH, outline=None)
    draw_rect(img, 40, 20, 3, 3, OL, outline=None)
    draw_rect(img, 41, 21, 1, 1, WH, outline=None)
    
    # Tail waving to the left
    draw_rect(img, 20, 27, 8, 6, SL_D)
    draw_rect(img, 14, 28, 6, 4, SL_D)
    draw_rect(img, 8, 29, 6, 2, SL_L)
    
    return img

# Save them to verify
os.makedirs("scratch", exist_ok=True)
draw_crab().save("scratch/test_crab.png")
draw_turtle().save("scratch/test_turtle.png")
draw_jellyfish().save("scratch/test_jellyfish.png")
draw_tadpole().save("scratch/test_tadpole.png")
print("Done drawing custom sprites.")
