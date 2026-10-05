shader_type canvas_item;

// Draws an outline around a sprite's visible pixels while `enabled` is true.
// The texture needs a few pixels of transparent margin for the outline to fit in.

uniform bool enabled = false;
uniform vec4 outline_color : source_color = vec4(1.0, 0.92, 0.55, 1.0);
uniform float width : hint_range(0.0, 8.0) = 2.0;

// The vertex colour carries the node's tint (e.g. the patience Modulate), so keep it to apply at the end.
varying vec4 tint;

void vertex() {
    tint = COLOR;
}

void fragment() {
    vec4 base = texture(TEXTURE, UV);
    vec4 result = base;

    if (enabled && base.a < 1.0) {
        // Is any pixel `width` away in one of 8 directions visible? Then this pixel is on the outline.
        vec2 offset = TEXTURE_PIXEL_SIZE * width;
        float neighbours = 0.0;
        neighbours += texture(TEXTURE, UV + vec2(offset.x, 0.0)).a;
        neighbours += texture(TEXTURE, UV - vec2(offset.x, 0.0)).a;
        neighbours += texture(TEXTURE, UV + vec2(0.0, offset.y)).a;
        neighbours += texture(TEXTURE, UV - vec2(0.0, offset.y)).a;
        neighbours += texture(TEXTURE, UV + offset).a;
        neighbours += texture(TEXTURE, UV - offset).a;
        neighbours += texture(TEXTURE, UV + vec2(offset.x, -offset.y)).a;
        neighbours += texture(TEXTURE, UV + vec2(-offset.x, offset.y)).a;

        float pulse = 0.75 + 0.25 * sin(TIME * 4.0);
        vec4 outline = vec4(outline_color.rgb, clamp(neighbours, 0.0, 1.0) * outline_color.a * pulse);
        result = mix(outline, base, base.a);
    }

    COLOR = result * tint;
}
