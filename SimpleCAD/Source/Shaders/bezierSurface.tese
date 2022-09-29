#version 460
layout( quads, equal_spacing, cw ) in;

uniform mat4 view;
uniform mat4 projection;

in vec4 tesc_colors[];
flat in int tesc_primitives[];
out vec4 tese_color;
out vec2 tese_tex;
flat out int tese_primitive;

void main() {
	vec4 p00 = gl_in[ 0 ].gl_Position;
	vec4 p10 = gl_in[ 1 ].gl_Position;
	vec4 p20 = gl_in[ 2 ].gl_Position;
	vec4 p30 = gl_in[ 3 ].gl_Position;
	vec4 p01 = gl_in[ 4 ].gl_Position;
	vec4 p11 = gl_in[ 5 ].gl_Position;
	vec4 p21 = gl_in[ 6 ].gl_Position;
	vec4 p31 = gl_in[ 7 ].gl_Position;
	vec4 p02 = gl_in[ 8 ].gl_Position;
	vec4 p12 = gl_in[ 9 ].gl_Position;
	vec4 p22 = gl_in[ 10 ].gl_Position;
	vec4 p32 = gl_in[ 11 ].gl_Position;
	vec4 p03 = gl_in[ 12 ].gl_Position;
	vec4 p13 = gl_in[ 13 ].gl_Position;
	vec4 p23 = gl_in[ 14 ].gl_Position;
	vec4 p33 = gl_in[ 15 ].gl_Position;

	float u = gl_TessCoord.x;
	float v = gl_TessCoord.y;

	float bu0 = (1.-u) * (1.-u) * (1.-u);
	float bu1 = 3. * u * (1.-u) * (1.-u);
	float bu2 = 3. * u * u * (1.-u);
	float bu3 = u * u * u;

	float bv0 = (1.-v) * (1.-v) * (1.-v);
	float bv1 = 3. * v * (1.-v) * (1.-v);
	float bv2 = 3. * v * v * (1.-v);
	float bv3 = v * v * v;

	vec4 p = bv0 * ( bu0*p00 + bu1*p01 + bu2*p02 + bu3*p03 ) 
	+ bv1 * ( bu0*p10 + bu1*p11 + bu2*p12 + bu3*p13 )
	+ bv2 * ( bu0*p20 + bu1*p21 + bu2*p22 + bu3*p23 )
	+ bv3 * ( bu0*p30 + bu1*p31 + bu2*p32 + bu3*p33 );

	gl_Position = p * view * projection;
	
	tese_color = tesc_colors[0];
	tese_tex = vec2(u, v);
	tese_primitive = tesc_primitives[0];
}