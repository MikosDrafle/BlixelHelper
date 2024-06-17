local af = {}

local dS = require("structs.drawable_sprite")
local dR = require("structs.drawable_rectangle")

af.depth = -math.huge+6
af.name = "BlixelHelper/AttachableFlower"
af.placements = {
    name = "Attachable Flower",
    data = {
        color = "FFFFFF"
    }
}

af.fieldInformation = {
    color = {
        fieldType = "color"
    }
}

af.width = 16
af.height = 16

function af.sprite(room, entity)
    local sprites = {}

    local x = entity.x
    local y = entity.y

    local sprite = dS.fromTexture("objects/attachableFlowers/idle00", entity)
    sprite:setColor(entity.color)
    math.randomseed(entity.x+entity.y)
    sprite.rotation = math.random()*360
    
    local rectSprite = dR.fromRectangle("fill", entity.x-1, entity.y-1, 2, 2, {1,1,1,1}):getDrawableSprite()

    table.insert(sprites, sprite)
    table.insert(sprites, rectSprite)

    return sprites
end

return af