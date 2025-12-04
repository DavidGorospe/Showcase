// Add (hard code) an orbiting (point or directional) light to the scene. Light
// the scene using the Blinn-Phong Lighting Model.
//
// Uniforms:
uniform mat4 view;
uniform mat4 proj;
uniform float animation_seconds;
uniform bool is_moon;
// Inputs:
in vec3 sphere_fs_in;
in vec3 normal_fs_in;
in vec4 pos_fs_in; 
in vec4 view_pos_fs_in; 
// Outputs:
out vec3 color;
// expects: PI, blinn_phong
void main()
{
  // Material properties
  vec3 ka = vec3(0.1, 0.1, 0.1); // Ambient
  vec3 kd = vec3(0.0, 0.0, 0.0); // Diffuse 
  vec3 ks = vec3(1.0, 1.0, 1.0); // Specular
  float shininess = 1000.0;

  // Set colour
  if (is_moon) {
    kd = vec3(0.5); // Gray
  } else {
    kd = vec3(0.0, 0.0, 1.0); // Blue 
  }

  kd *= 0.8 + 0.2 * (sphere_fs_in.y + 1.0); 

  float orbit_radius = 3.0;
  float orbit_speed = M_PI * 2.0 / 8.0; // 1 orbit every 8 sec
  vec3 light_pos_world = vec3(
    orbit_radius * cos(animation_seconds * orbit_speed),
    3.0,
    orbit_radius * sin(animation_seconds * orbit_speed)
  );

  vec3 light_pos_view = (view * vec4(light_pos_world, 1.0)).xyz;

  vec3 frag_pos_view = view_pos_fs_in.xyz;
  vec3 n = normalize(normal_fs_in);
  vec3 v = normalize(-frag_pos_view);
  vec3 l = normalize(light_pos_view - frag_pos_view);

  float spec_factor = 0.5 + 0.5 * sin(dot(pos_fs_in.xyz, vec3(1.0, 1.0, 0.0)));
  vec3 ks_mod = ks * spec_factor;

  color = blinn_phong(ka, kd, ks_mod, shininess, n, v, l);
}
