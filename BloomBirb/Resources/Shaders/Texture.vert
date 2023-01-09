#version 330 core

#include "shared.h"

void main()
{
    gl_Position =  vec4(u_TransMat * vec3(v_pos, 1), 1);

    f_uv = v_uv;
}
