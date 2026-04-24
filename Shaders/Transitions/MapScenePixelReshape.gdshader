shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float min_block_size = 1.0;
uniform float max_block_size = 20.0;
uniform float contrast_boost = 0.06;

void fragment() {
	vec2 viewport_size = 1.0 / SCREEN_PIXEL_SIZE;
	float block_size = mix(min_block_size, max_block_size, smoothstep(0.0, 1.0, progress));
	vec2 pixel_block = vec2(
		max(1.0, floor(block_size)),
		max(1.0, floor(block_size))
	);

	vec2 block_uv = floor((SCREEN_UV * viewport_size) / pixel_block) * pixel_block / viewport_size;
	vec2 centered_uv = block_uv + (pixel_block * 0.5 * SCREEN_PIXEL_SIZE);
	vec2 sample_uv = clamp(centered_uv, vec2(0.0), vec2(1.0));

	vec4 scene_color = texture(screen_texture, sample_uv);
	vec3 contrasted = mix(scene_color.rgb, pow(scene_color.rgb, vec3(0.92)), contrast_boost * progress);
	COLOR = vec4(contrasted, 1.0);
}
