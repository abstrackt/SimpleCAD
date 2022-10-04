#version 400
in vec4 vs_color;
in vec2 vs_uv;
out vec4 color;

uniform sampler2D mask;
uniform int trim;
uniform int target;

void main()
{
	if (trim != 0) 
	{
		float sampled = texture(mask, vs_uv).r * 255.0;

		if (sampled == target)
			discard;
	}

	color = vs_color;
}
