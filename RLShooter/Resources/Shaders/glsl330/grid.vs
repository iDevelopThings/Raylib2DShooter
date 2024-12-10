#version 330

layout(location = 0) in vec3 vertexPosition;  // Vertex position input
layout(location = 1) in vec2 vertexTexCoord;  // Texture coordinate input

uniform mat4 mvp;  // Model-View-Projection matrix

out vec3 fragPos;  // Fragment position to be passed to the fragment shader

void main() {
    fragPos = vertexPosition;  // Pass the position to the fragment shader
    gl_Position = mvp * vec4(vertexPosition, 1.0);  // Compute the vertex position
}
