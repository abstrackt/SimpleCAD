#version 400
in vec4 tese_pos;
in vec4 tese_color;
in vec2 tese_uv;

layout (location = 0) out vec4 color;
layout (location = 1) out float height;

uniform sampler2D mask;
uniform int trim;
uniform int target;

void main()
{
	if (trim != 0) 
	{
		float sampled = texture(mask, tese_uv).r * 255.0;

		if (sampled == target)
			discard;
	}

	color = tese_color;
	height = tese_pos.y;
}
