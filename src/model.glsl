// Construct the model transformation matrix. The moon should orbit around the
// origin. The other object should stay still.
//
// Inputs:
//   is_moon  whether we're considering the moon
//   time  seconds on animation clock
// Returns affine model transformation as 4x4 matrix
//
// expects: identity, rotate_about_y, translate, PI
mat4 model(bool is_moon, float time)
{
  mat4 I = identity();
  float orbit_radius = 3.0;        // Orbit distance
  float orbit_speed = 3.14159265 / 2.0; // 1 orbit per 4s
  float scale_factor = 0.3;        // Shrink moon

  // Scale
  mat4 S = uniform_scale(scale_factor);

  // Translate
  mat4 T = translate(vec3(orbit_radius, 0.0, 0.0));

  // Rotate around origin
  mat4 R = rotate_about_y(-time * orbit_speed);

  // Transform
  mat4 moon_transform = R * T * S;

  return I + float(is_moon) * (moon_transform - I);
}
