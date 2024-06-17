local drawableNinePatch = require("structs.drawable_nine_patch")

local factory = {}

factory.name = "BlixelHelper/FactoryBounceBlock"
factory.depth = -100
factory.minimumSize = {16, 16}

factory.placements = {
    name = "Factory Bounce Block",
    placementType = "rectangle",
    data = {
        width = 16,
        height = 16,
        repulseFactor = 3.2,
        moveDirections = 8,
        reformTime = 3.0
    }
}

factory.fieldInformation = {
    moveDirections = {
        fieldType="integer",
        minimumValue=4,
        maximumValue=360
    }
}

function factory.sprite(room, entity)
    local sprites = {}
    local x,y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local ninePatch = drawableNinePatch.fromTexture("objects/bouncer/idle00", {}, x, y, width, height)
    table.insert(sprites, ninePatch)

    return sprites
end

return factory
