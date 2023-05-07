layout(location = 0) in vec2 v_pos;
layout(location = 1) in vec4 v_col;
layout(location = 2) in vec2 v_uv;
layout(location = 3) in vec2 v_texRegionOrigin;
layout(location = 4) in vec2 v_texRegionSize;
layout(location = 5) in float v_depth;

out vec2 f_uv;
out vec4 f_col;

uniform mat3 u_TransMat;
layout(location = 34) uniform vec2 u_textureSize;