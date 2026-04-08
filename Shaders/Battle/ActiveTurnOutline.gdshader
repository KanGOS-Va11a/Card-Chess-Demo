shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(1.0, 0.22, 0.22, 0.95);
uniform float outline_size = 1.0;

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	float alpha = tex.a;
	vec2 pixel = TEXTURE_PIXEL_SIZE * outline_size;

	float outline = 0.0;
	outline += texture(TEXTURE, UV + vec2(pixel.x, 0.0)).a;
	outline += texture(TEXTURE, UV + vec2(-pixel.x, 0.0)).a;
	outline += texture(TEXTURE, UV + vec2(0.0, pixel.y)).a;
	outline += texture(TEXTURE, UV + vec2(0.0, -pixel.y)).a;
	outline += texture(TEXTURE, UV + vec2(pixel.x, pixel.y)).a;
	outline += texture(TEXTURE, UV + vec2(pixel.x, -pixel.y)).a;
	outline += texture(TEXTURE, UV + vec2(-pixel.x, pixel.y)).a;
	outline += texture(TEXTURE, UV + vec2(-pixel.x, -pixel.y)).a;

	if (alpha <= 0.001 && outline > 0.0) {
		COLOR = outline_color;
	} else {
		COLOR = tex;
	}
}
