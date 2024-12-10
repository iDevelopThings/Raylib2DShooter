#version 330

uniform vec3 cameraPos;     // The camera position
uniform float maxDistance;  // Maximum distance for the fade effect
uniform float lineWidth;    // Width of the grid lines
uniform int gridSize;       // Size of the grid spacing

in vec3 fragPos;

out vec4 finalColor;

void main() {
    // Calculate distance from camera to fragment
    float distance = length(fragPos - cameraPos);

    // Calculate alpha based on distance for fading effect
    float alpha = clamp(1.0 - (distance / maxDistance), 0.0, 1.0);

    // Compute grid lines using mod function
    vec2 gridPos = mod(fragPos.xz, gridSize);

    // Manually calculate the blend factor for grid lines
    float edgeDistX = 1.0 - abs(gridPos.x - gridSize * 0.5) / (lineWidth * 0.5);
    float edgeDistY = 1.0 - abs(gridPos.y - gridSize * 0.5) / (lineWidth * 0.5);

    // Clamp the values to create sharp edges
    edgeDistX = clamp(edgeDistX, 0.0, 1.0);
    edgeDistY = clamp(edgeDistY, 0.0, 1.0);

    // Calculate final alpha based on the proximity to grid lines
    float gridAlpha = max(edgeDistX, edgeDistY) * alpha;

    // Set the grid color with fading alpha
    finalColor = vec4(vec3(0.4), gridAlpha);
}
