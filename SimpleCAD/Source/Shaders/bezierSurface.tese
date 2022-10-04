#version 460
layout( quads, equal_spacing, cw ) in;

uniform mat4 view;
uniform mat4 projection;

in vec4 tesc_colors[];
in vec2 tesc_uvs[];
out vec4 tese_color;
out vec2 tese_uv;

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

	vec2 uv00 = tesc_uvs[0];
	vec2 uv10 = tesc_uvs[1];
	vec2 uv20 = tesc_uvs[2];
	vec2 uv30 = tesc_uvs[3];
	vec2 uv01 = tesc_uvs[4];
	vec2 uv11 = tesc_uvs[5];
	vec2 uv21 = tesc_uvs[6];
	vec2 uv31 = tesc_uvs[7];
	vec2 uv02 = tesc_uvs[8];
	vec2 uv12 = tesc_uvs[9];
	vec2 uv22 = tesc_uvs[10];
	vec2 uv32 = tesc_uvs[11];
	vec2 uv03 = tesc_uvs[12];
	vec2 uv13 = tesc_uvs[13];
	vec2 uv23 = tesc_uvs[14];
	vec2 uv33 = tesc_uvs[15];

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

	vec2 uv = bv0 * ( bu0*uv00 + bu1*uv01 + bu2*uv02 + bu3*uv03 ) 
	+ bv1 * ( bu0*uv10 + bu1*uv11 + bu2*uv12 + bu3*uv13 )
	+ bv2 * ( bu0*uv20 + bu1*uv21 + bu2*uv22 + bu3*uv23 )
	+ bv3 * ( bu0*uv30 + bu1*uv31 + bu2*uv32 + bu3*uv33 );

	gl_Position = p * view * projection;
	
	tese_color = tesc_colors[0];
	tese_uv = uv;
}