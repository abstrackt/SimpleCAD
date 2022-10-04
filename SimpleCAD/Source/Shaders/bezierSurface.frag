#version 460
in vec4 tese_color;
in vec2 tese_uv;
out vec4 color;

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
}
