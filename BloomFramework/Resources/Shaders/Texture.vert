#version 430 core

#include "shared.h"

layout(location = 0) in vec2 v_pos;
layout(location = 1) in vec4 v_col;
layout(location = 2) in vec2 v_uv;
layout(location = 3) in float v_depth;

out FragData 
{
    vec2 f_uv;
    vec4 f_col;
};

void main()
{
    gl_Position =  vec4((u_projMatrix * vec3(v_pos, 1)).xy, v_depth, 1);

    f_uv = v_uv;
    f_col = v_col;
}
