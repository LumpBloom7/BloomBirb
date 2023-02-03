#version 330 core

#include "shared.h"

void main()
{
    gl_Position =  vec4(v_pos, 0, 1);

    f_uv = v_uv;
}
