//Specifying the version like in our vertex shader.
#version 330 core

in vec2 f_uv;
in vec4 f_col;

uniform sampler2D u_Texture0;

out vec4 FragColor;

uniform bool u_Circle;
uniform float u_ScreenSpaceCentreY;
uniform float u_ScreenSpaceCentreX;
uniform float u_CircleRadius;

void main()
{
    // Safe to branch as all fragments will travel through this branch
    // Turn it into a circle if the circle flag is set
    if(u_Circle){
        float dist = distance(gl_FragCoord.xy, vec2(u_ScreenSpaceCentreX, u_ScreenSpaceCentreY));

        FragColor = vec4(texture(u_Texture0, f_uv).rgb, clamp( 1 - (dist - u_CircleRadius) / 1, 0, 1)) * u_Color;
        return;
    }

    //Here we are setting our output variable, for which the name is not important.
    FragColor = texture(u_Texture0, f_uv) * f_col;
}
