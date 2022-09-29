#version 460
in vec4 tese_color;
in vec2 tese_tex;
flat in int tese_primitive;
out vec4 color;

uniform int patches_u;
uniform int patches_v;

uniform sampler2D mask;
uniform int trim;
uniform int target;

void main()
{
	int u_idx = tese_primitive%patches_u;
    int v_idx = tese_primitive/patches_u;

	vec2 tex_coords = vec2((tese_tex.x + u_idx)/patches_u, (tese_tex.y + v_idx)/patches_v);

	if (trim != 0) 
	{
		float sampled = texture(mask, tex_coords).r * 255.0;

		if (sampled == target)
			discard;
	}

	color = tese_color;
}
