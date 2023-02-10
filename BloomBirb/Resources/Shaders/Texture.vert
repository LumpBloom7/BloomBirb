#version 330 core

#include "shared.h"

void main()
{
    gl_Position =  vec4(v_pos, v_depth, 1);

    f_uv = v_uv;
    f_col = v_col;
}
