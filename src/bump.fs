// Set the pixel color using Blinn-Phong shading (e.g., with constant blue and
// gray material color) with a bumpy texture.
// 
// Uniforms:
uniform mat4 view;
uniform mat4 proj;
uniform float animation_seconds;
uniform bool is_moon;
// Inputs:
//                     linearly interpolated from tessellation evaluation shader
//                     output
in vec3 sphere_fs_in;
in vec3 normal_fs_in;
in vec4 pos_fs_in; 
in vec4 view_pos_fs_in; 
// Outputs:
//               rgb color of this pixel
out vec3 color;
// expects: model, blinn_phong, bump_height, bump_position,
// improved_perlin_noise, tangent
void main()
{
  // Material properties
  vec3 ka = vec3(0.1, 0.1, 0.1); // Ambient
  vec3 kd = vec3(0.0, 0.0, 0.0); // Diffuse 
  vec3 ks = vec3(1.0, 1.0, 1.0); // Specular
  float shininess = 1000.0;

  // Model
  mat4 M = model(is_moon, animation_seconds);

  vec3 model_pos = (M * vec4(sphere_fs_in, 1.0)).xyz;
  vec3 normal_model = normalize((M * vec4(normal_fs_in, 0.0)).xyz);

  if (is_moon) {
    kd = vec3(0.5);
  } else {
    kd = vec3(0.0, 0.0, 1.0);
  }

  vec3 T, B;
  tangent(normal_model, T, B);

  float eps = 0.001;
  vec3 offset_x = T * eps;
  vec3 offset_y = B * eps;

  vec3 p_center = model_pos + normal_model * bump_height(is_moon, model_pos);
  vec3 p_x = model_pos + offset_x + normal_model * bump_height(is_moon, model_pos + offset_x);
  vec3 p_y = model_pos + offset_y + normal_model * bump_height(is_moon, model_pos + offset_y);

  vec3 displaced_normal = normalize(cross(p_x - p_center, p_y - p_center));
  vec3 displaced_pos = p_center;

  float orbit_radius = 3.0;
  float orbit_speed = M_PI * 2.0 / 4.0; // 1 orbit per 8 sec
  float orbit_angle = animation_seconds * orbit_speed;
  vec3 light_pos_world = vec3(
    orbit_radius * cos(orbit_angle),
    3.0,
    orbit_radius * sin(orbit_angle)
  );

  vec3 frag_pos_view = (view * vec4(displaced_pos, 1.0)).xyz;
  vec3 light_pos_view = (view * vec4(light_pos_world, 1.0)).xyz;

  vec3 n = displaced_normal;
  vec3 v = normalize(-frag_pos_view);
  vec3 l = normalize(light_pos_view - frag_pos_view);

  color = blinn_phong(ka, kd, ks, shininess, n, v, l);
}
