// Compute Blinn-Phong Shading given a material specification, a point on a
// surface and a light direction. Assume the light is white and has a low
// ambient intensity.
//
// Inputs:
//   ka  rgb ambient color
//   kd  rgb diffuse color
//   ks  rgb specular color
//   p  specular exponent (shininess)
//   n  unit surface normal direction
//   v  unit direction from point on object to eye
//   l  unit light direction
// Returns rgb color
vec3 blinn_phong(
  vec3 ka,
  vec3 kd,
  vec3 ks,
  float p,
  vec3 n,
  vec3 v,
  vec3 l)
{
  // Ensure all directions are normalized
  n = normalize(n);
  v = normalize(v);
  l = normalize(l);

  // Ambient 
  vec3 ambient = 0.1 * ka;

  // Diffuse 
  float diff = max(dot(n, l), 0.0);
  vec3 diffuse = diff * kd;

  // Specular 
  vec3 h = normalize(l + v);          
  float spec = pow(max(dot(n, h), 0.0), p);
  vec3 specular = spec * ks;

  return ambient + diffuse + specular;
}


