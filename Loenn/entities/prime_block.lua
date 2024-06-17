local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local prime = {}

prime.name = "BlixelHelper/PrimeBlock"
prime.depth = -100
prime.minimumSize = {16, 16}

prime.placements = {
    name = "Prime Block",
    placementType = "rectangle",
    data = {
        width = 16,
        height = 16,
    }
}

function prime.sprite(room, entity)
    local sprites = {}
    local x,y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local ninePatch = drawableNinePatch.fromTexture("objects/primeBlocks/block", {}, x, y, width, height)
    local cap = drawableSprite.fromTexture("objects/primeBlocks/cap", {x = entity.x + (entity.width/2), y = entity.y-4})

    local text = drawableSprite.fromTexture("objects/primeBlocks/text", {x = entity.x + (entity.width/2), y = entity.y + (entity.height/2)})

    text:setScale((entity.width/36) - (4/36), (entity.height/16) - (4/16))
    
    return {ninePatch, text, cap}
end

return prime
