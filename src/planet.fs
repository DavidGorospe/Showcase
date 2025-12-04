// Generate a procedural planet and orbiting moon. Use layers of (improved)
// Perlin noise to generate planetary features such as vegetation, gaseous
// clouds, mountains, valleys, ice caps, rivers, oceans. Don't forget about the
// moon. Use `animation_seconds` in your noise input to create (periodic)
// temporal effects.
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
// expects: model, blinn_phong, bump_height, bump_position,
// improved_perlin_noise, tangent, random2
void main()
{
  // Material properties
  vec3 ka = vec3(0.3, 0.3, 0.3); // Ambient
  vec3 kd = vec3(0.0, 0.0, 0.0); // Diffuse 
  vec3 ks = vec3(1.0, 1.0, 1.0); // Specular
  float shininess = 1000.0;

  // Model
  mat4 M = model(is_moon, animation_seconds);            
  vec3 model_pos = (M * vec4(sphere_fs_in, 1.0)).xyz;    
  vec3 normal_model = normalize((M * vec4(normal_fs_in, 0.0)).xyz);

  vec3 tex_p = normalize(sphere_fs_in); 
  
  // Rotate Earth slowly for more dynamic view
  if (!is_moon) {
    float earth_rotation_speed = M_PI * 2.0 / 60.0; // 1 rotation per 60s
    float earth_angle = animation_seconds * earth_rotation_speed;
    float c = cos(earth_angle);
    float s = sin(earth_angle);
    mat4 rot_earth = mat4(1.0);
    rot_earth[0][0] =  c;  rot_earth[0][2] =  s;
    rot_earth[2][0] = -s;  rot_earth[2][2] =  c;
    tex_p = (rot_earth * vec4(tex_p, 0.0)).xyz;
  }
  
  if (is_moon) {
    float orbit_speed = M_PI * 2.0 / 8.0; // 1 orbit per 8s
    float orbit_angle = animation_seconds * orbit_speed;
    float c = cos(orbit_angle);
    float s = sin(orbit_angle);
    mat4 rot = mat4(1.0);
    rot[0][0] =  c;  rot[0][2] =  s;
    rot[2][0] = -s;  rot[2][2] =  c;
    tex_p = (rot * vec4(tex_p, 0.0)).xyz;
  }

  // Layered terrain noise (improved Perlin)
  float t = animation_seconds;

  float freq0 = 0.0;
  if (is_moon) {
    freq0 = 2.5;
  } else {
    freq0 = 1.5;
  }
  float base = 0.0;
  float amp = 1.0;
  float tot = 0.0;
  float freq = freq0;
  for (int i = 0; i < 5; ++i) {
    float n = improved_perlin_noise(tex_p * freq + vec3(0.0, t * 0.02, 0.0));
    base += n * amp;
    tot += amp;
    freq *= 2.0;
    amp *= 0.5;
  }
  normalize(base);

  float terrain = sign(base) * pow(abs(base), 1.15);
  float sea_level = is_moon ? -0.3 : -0.05;   
  float mountain_level = 0.45;                
  float ice_lat_cutoff = 0.9;                 

  // Small-scale details (rocks, craters, rivers)
  float detail = 0.0;
  if (is_moon) {
    freq = 12.0;
  } else {
    freq = 6.0;
  }
  amp = 0.5;
  for (int i = 0; i < 3; ++i) {
    detail += improved_perlin_noise(tex_p * freq + vec3(t * 0.1 * float(i), 0.0, 0.0)) * amp;
    freq *= 2.0;
    amp *= 0.5;
  }
  
  float rivers = 0.0;
  if (!is_moon) {
    float river_freq = 8.0;
    for (int i = 0; i < 2; ++i) {
      rivers += abs(improved_perlin_noise(tex_p * river_freq + vec3(t * 0.03, 0.0, 0.0))) * 0.3;
      river_freq *= 2.0;
    }
    rivers = smoothstep(0.15, 0.25, rivers);
  }
  
  float craters = 0.0;
  if (is_moon) {
    float crater_freq = 15.0;
    for (int i = 0; i < 3; ++i) {
      float crater_noise = improved_perlin_noise(tex_p * crater_freq);
      craters += smoothstep(0.3, 0.7, crater_noise) * 0.4;
      crater_freq *= 1.8;
    }
  }

  float height_field = clamp(terrain + 0.35 * detail - rivers * 0.1 + craters * 0.15, -1.0, 1.0);

  float water_mask = smoothstep(sea_level - 0.02, sea_level + 0.02, height_field); // >0 => land, <0 => water
  float is_land = step(sea_level, height_field);

  float lat = clamp(model_pos.y, -1.0, 1.0); 
  float abs_lat = abs(lat);

  // Surface colors: ocean, shallow water, land, mountains, ice 
  vec3 ocean_color = vec3(0.01, 0.05, 0.15);
  vec3 shallow_color = vec3(0.08, 0.25, 0.35);
  vec3 land_low = vec3(0.15, 0.45, 0.12);
  vec3 land_high = vec3(0.55, 0.35, 0.15);
  vec3 mountain_color = vec3(0.55, 0.50, 0.45);
  vec3 ice_color = vec3(0.98, 1.0, 1.0);

  float veg = smoothstep(0.0, 0.6, 1.0 - abs_lat); 

  vec3 base_color;
  if (is_moon) {
    // Moon
    vec3 moon_dark = vec3(0.25, 0.25, 0.28);
    vec3 moon_bright = vec3(0.85, 0.85, 0.88);
    base_color = mix(moon_dark, moon_bright, smoothstep(-0.6, 0.8, height_field));
    base_color *= 0.85 + 0.15 * detail;
  } else {
    // Water/land
    float shallow = smoothstep(sea_level - 0.02, sea_level + 0.05, height_field);
    vec3 land_mix = mix(land_low * veg, land_high, smoothstep(0.0, 0.6, height_field));
    base_color = mix(ocean_color, land_mix, shallow);
    base_color = mix(base_color, vec3(0.02, 0.15, 0.25), rivers * 0.6);
    // Mountains/ice
    float mountain_mask = smoothstep(mountain_level - 0.05, mountain_level + 0.2, height_field);
    base_color = mix(base_color, mountain_color, mountain_mask);
    float ice_mask = smoothstep(ice_lat_cutoff - 0.05, ice_lat_cutoff + 0.05, abs_lat);
    base_color = mix(base_color, ice_color, ice_mask * smoothstep(0.55, 0.9, height_field));
  }

  // Bumps
  vec3 T, B;
  tangent(normal_model, T, B);

  float eps = 0.001;  
  vec3 offX = T * eps;
  vec3 offY = B * eps;

  float h_c = bump_height(is_moon, model_pos);
  float h_x = bump_height(is_moon, model_pos + offX);
  float h_y = bump_height(is_moon, model_pos + offY);

  vec3 p_c = model_pos + normal_model * h_c;
  vec3 p_x = model_pos + offX + normal_model * h_x;
  vec3 p_y = model_pos + offY + normal_model * h_y;

  vec3 displaced_normal = normalize(cross(p_x - p_c, p_y - p_c));

  vec3 displaced_pos_model = p_c;

  vec3 sun_pos_world = vec3(10.0, 5.0, 8.0);
  
  // Moon orbit
  float orbit_radius = 3.0;
  float orbit_speed = M_PI * 2.0 / 8.0; // 1 orbit per 8s
  float orbit_angle = animation_seconds * orbit_speed;
  vec3 moon_pos_world = vec3(orbit_radius * cos(orbit_angle), 0.0, orbit_radius * sin(orbit_angle));
  
  // Use sun as main light, moon as secondary light
  vec3 frag_pos_view = (view * vec4(displaced_pos_model, 1.0)).xyz;
  vec3 sun_pos_view = (view * vec4(sun_pos_world, 1.0)).xyz;
  vec3 moon_pos_view = (view * vec4(moon_pos_world, 1.0)).xyz;

  vec3 v = normalize(-frag_pos_view);
  vec3 l_sun = normalize(sun_pos_view - frag_pos_view);
  vec3 l_moon = normalize(moon_pos_view - frag_pos_view);
  vec3 nrm = normalize(displaced_normal);

  float shadow = 1.0;
  if (!is_moon) {
    vec3 to_moon = normalize(moon_pos_world - displaced_pos_model);
    vec3 to_sun_dir = normalize(sun_pos_world - displaced_pos_model);
    float moon_occlusion = dot(to_moon, to_sun_dir);
    if (moon_occlusion > 0.7 && length(moon_pos_world - displaced_pos_model) < 4.0) {
      float dist_to_moon = length(moon_pos_world - displaced_pos_model);
      shadow = 0.3 + 0.7 * smoothstep(2.5, 4.0, dist_to_moon);
    }
  }
  
  // Main lighting from sun
  kd = base_color * 1.8;
  vec3 lit = blinn_phong(ka, kd, ks, shininess, nrm, v, l_sun) * shadow;
  
  // Enhanced specular for water/ocean reflections 
  if (!is_moon) {
    float water_spec = smoothstep(sea_level - 0.05, sea_level + 0.02, height_field);
    water_spec = 1.0 - water_spec; 
    
    float wave_noise = 0.0;
    float wave_freq = 4.0;
    for (int i = 0; i < 2; ++i) {
      wave_noise += improved_perlin_noise(tex_p * wave_freq + vec3(0.0, t * 0.3, 0.0)) * 0.5;
      wave_freq *= 2.0;
    }
    float wave_height = wave_noise * 0.02;
    
    vec3 h = normalize(l_sun + v);
    float water_highlight = pow(max(dot(nrm, h), 0.0), shininess * 2.0);
    lit += vec3(1.0, 1.0, 1.0) * water_highlight * water_spec * (0.4 + wave_height * 2.0);
  }
  
  // Add Earth's glow 
  if (!is_moon) {
    float rim = 1.0 - max(dot(nrm, v), 0.0);
    rim = pow(rim, 1.8);
    vec3 atmosphere_color = vec3(0.4, 0.6, 1.0) * 0.8;
    float sun_factor = max(dot(nrm, l_sun), 0.0);
    lit += atmosphere_color * rim * (0.4 + 0.6 * sun_factor);
    
    // Add subtle inner glow
    float inner_rim = 1.0 - max(dot(nrm, v), 0.0);
    inner_rim = pow(inner_rim, 4.0);
    lit += vec3(0.2, 0.4, 0.7) * inner_rim * 0.3;
    
    // Clouds
    float cloud_noise = 0.0;
    float cloud_freq = 2.0;
    float cloud_amp = 1.0;
    for (int i = 0; i < 4; ++i) {
      cloud_noise += improved_perlin_noise(tex_p * cloud_freq + vec3(0.0, t * 0.05, 0.0)) * cloud_amp;
      cloud_freq *= 2.0;
      cloud_amp *= 0.5;
    }
    cloud_noise = smoothstep(0.2, 0.6, cloud_noise);
    
    // Add second cloud layer for depth
    float cloud_layer2 = 0.0;
    cloud_freq = 1.5;
    cloud_amp = 0.7;
    for (int i = 0; i < 3; ++i) {
      cloud_layer2 += improved_perlin_noise(tex_p * cloud_freq + vec3(0.0, t * 0.03, 0.0)) * cloud_amp;
      cloud_freq *= 2.0;
      cloud_amp *= 0.5;
    }
    cloud_layer2 = smoothstep(0.25, 0.55, cloud_layer2);
    cloud_noise = max(cloud_noise, cloud_layer2 * 0.6);
    
    vec3 cloud_color = vec3(1.0, 1.0, 1.0) * 0.35;
    float cloud_lighting = max(dot(nrm, l_sun), 0.0);
    lit = mix(lit, lit + cloud_color * cloud_lighting, cloud_noise * 0.5);
    
    // City lights on dark side
    float night_side = 1.0 - max(dot(nrm, l_sun), 0.0);
    night_side = smoothstep(0.3, 0.7, night_side);
    float city_density = 0.0;
    float city_freq = 20.0;
    for (int i = 0; i < 2; ++i) {
      float city_noise = improved_perlin_noise(tex_p * city_freq);
      city_density += smoothstep(0.6, 0.9, city_noise) * 0.5;
      city_freq *= 2.0;
    }
    city_density = smoothstep(0.2, 0.8, city_density);
    vec3 city_color = mix(vec3(1.0, 0.9, 0.7), vec3(0.8, 0.9, 1.0), city_density);
    lit += city_color * night_side * city_density * 0.4;
    
    // Aurora borealis effect at poles
    float aurora_strength = 0.0;
    if (abs_lat > 0.7) {
      float aurora_noise = 0.0;
      float aurora_freq = 3.0;
      for (int i = 0; i < 3; ++i) {
        aurora_noise += improved_perlin_noise(tex_p * aurora_freq + vec3(0.0, t * 0.1, 0.0)) * 0.5;
        aurora_freq *= 1.5;
      }
      aurora_strength = smoothstep(0.3, 0.7, aurora_noise) * smoothstep(0.7, 0.95, abs_lat);
      aurora_strength *= night_side * 0.6;
      vec3 aurora_color = mix(vec3(0.2, 1.0, 0.4), vec3(0.4, 0.6, 1.0), abs_lat);
      lit += aurora_color * aurora_strength;
    }
  }
  
  if (is_moon) {
    // Moon glow
    float moon_glow = 1.0 - max(dot(nrm, v), 0.0);
    moon_glow = pow(moon_glow, 1.5);
    lit += vec3(0.9, 0.9, 1.0) * moon_glow * 0.15;
    
    // Moon darken the side facing away from sun
    vec3 to_sun = normalize(sun_pos_world - moon_pos_world);
    vec3 to_earth = normalize(-moon_pos_world);
    float phase = dot(to_sun, to_earth);
    float phase_factor = 0.3 + 0.7 * smoothstep(-0.3, 0.3, phase);
    lit *= phase_factor;
    
    float crater_shadow = smoothstep(0.4, 0.8, craters);
    lit *= 0.9 + 0.1 * (1.0 - crater_shadow * max(dot(nrm, l_sun), 0.0));
    
    // Light reflected from Earth onto moon
    vec3 earth_to_moon = normalize(moon_pos_world);
    float earthshine = max(dot(nrm, -earth_to_moon), 0.0);
    earthshine = pow(earthshine, 3.0);
    lit += vec3(0.1, 0.2, 0.4) * earthshine * 0.2;
  } else {
    // Earth receives some light from moon (subtle)
    float moon_light = max(dot(nrm, l_moon), 0.0);
    lit += vec3(0.8, 0.85, 1.0) * moon_light * 0.1;
  }
  
  if (!is_moon) {
    vec3 view_dir = normalize(displaced_pos_model);
    vec3 sun_dir = normalize(sun_pos_world);
    float sun_dot_view = dot(sun_dir, view_dir);
    
    float rayleigh = pow(max(sun_dot_view, 0.0), 2.0);
    vec3 scatter_color = mix(
      vec3(0.3, 0.5, 1.0),  // Blue
      vec3(1.0, 0.6, 0.2),   // Orange/red at horizon
      smoothstep(0.0, -0.3, sun_dot_view)
    );
    
    float rim_factor = 1.0 - max(dot(nrm, v), 0.0);
    rim_factor = pow(rim_factor, 1.5);
    lit += scatter_color * rayleigh * rim_factor * 0.3;
    
    float mie = pow(max(sun_dot_view, 0.0), 8.0);
    lit += vec3(1.0, 0.95, 0.8) * mie * rim_factor * 0.2;
  }
  
  lit = pow(lit, vec3(0.9)); 
  
  float dist_to_origin = length(displaced_pos_model);
  float rim_factor_space = 1.0 - max(dot(nrm, v), 0.0);
  rim_factor_space = pow(rim_factor_space, 1.5);
  float space_factor = max(rim_factor_space, 0.3); 
  // Nebula effect visible around edges and through atmosphere
  vec3 nebula_color = vec3(0.0);
  vec3 nebula_dir = normalize(displaced_pos_model);
  float nebula_noise = 0.0;
  float nebula_freq = 0.3;
  float nebula_amp = 1.0;
  for (int i = 0; i < 4; ++i) {
    nebula_noise += improved_perlin_noise(nebula_dir * nebula_freq + vec3(t * 0.02, t * 0.015, 0.0)) * nebula_amp;
    nebula_freq *= 2.0;
    nebula_amp *= 0.5;
  }
  nebula_noise = smoothstep(0.1, 0.4, nebula_noise);
  
  vec3 nebula_purple = vec3(0.6, 0.3, 0.8);
  vec3 nebula_blue = vec3(0.3, 0.6, 1.0);
  vec3 nebula_pink = vec3(1.0, 0.5, 0.7);
  vec3 nebula_cyan = vec3(0.4, 0.9, 1.0);
  
  float color_shift = sin(t * 0.15) * 0.5 + 0.5;
  nebula_color = mix(nebula_purple, nebula_blue, color_shift);
  nebula_color = mix(nebula_color, nebula_pink, nebula_noise * 0.5);
  nebula_color = mix(nebula_color, nebula_cyan, (1.0 - nebula_noise) * 0.4);
  
  // Make nebula visible in rim areas and through atmosphere
  float nebula_visibility = rim_factor_space * 0.8 + 0.2;
  nebula_color *= nebula_noise * nebula_visibility * 0.4;
  
  // Add nebula to atmosphere glow
  if (!is_moon) {
    float atm_rim = 1.0 - max(dot(nrm, v), 0.0);
    atm_rim = pow(atm_rim, 1.5);
    nebula_color += nebula_color * atm_rim * 0.3;
  }
  
  vec3 to_sun = normalize(sun_pos_view - frag_pos_view);
  float sun_angle = dot(to_sun, v);
  float lens_flare = 0.0;
  
  if (sun_angle > 0.7) {
    float flare_dist = 1.0 - sun_angle;
    lens_flare += exp(-flare_dist * 20.0) * 2.0;
    
    vec3 up = vec3(0.0, 1.0, 0.0);
    vec3 right = normalize(cross(v, up));
    float flare2 = abs(dot(to_sun, right));
    lens_flare += exp(-flare2 * 20.0) * 0.8;
    
    float streak = abs(dot(to_sun, right));
    lens_flare += exp(-streak * 12.0) * 0.6;
    
    float halo = 1.0 - sun_angle;
    lens_flare += exp(-halo * 30.0) * 1.0;
    
    vec3 flare_color = mix(
      vec3(1.0, 0.95, 0.8),  // Warm white
      vec3(1.0, 0.7, 0.5),   // Orange
      smoothstep(0.7, 0.95, sun_angle)
    );
    
    float surface_flare = rim_factor_space * 0.5;
    lens_flare += surface_flare * smoothstep(0.7, 0.9, sun_angle);
    
    lit += flare_color * lens_flare * 0.8;
  }
  
  vec3 to_sun_world = normalize(sun_pos_world - displaced_pos_model);
  float sun_visibility = 0.0;
  if (space_factor > 0.4 && dist_to_origin > 4.0) {
    // Check if we're looking toward sun
    float sun_dot = dot(normalize(displaced_pos_model), normalize(sun_pos_world));
    if (sun_dot > 0.98) {
      float sun_size = smoothstep(0.98, 0.995, sun_dot);
      sun_visibility = sun_size * space_factor;
      
      float corona = smoothstep(0.98, 0.99, sun_dot);
      sun_visibility += corona * 0.3;
      
      vec3 sun_color = vec3(1.0, 0.95, 0.85);
      lit += sun_color * sun_visibility * 1.5;
    }
  }
  
  // Stars visible on surfaces and in rim areas
  vec3 star_dir = normalize(displaced_pos_model);
  vec2 star_hash = random2(star_dir * 300.0);
  float star_brightness = smoothstep(0.96, 1.0, star_hash.x);
  
  // Twinkling effect
  float twinkle_speed = 4.0 + star_hash.y * 3.0;
  float twinkle = sin(t * twinkle_speed + star_hash.y * 20.0) * 0.5 + 0.5;
  star_brightness *= 0.5 + 0.5 * twinkle;
  
  // More color variation
  vec3 star_color = vec3(1.0, 1.0, 1.0);
  if (star_hash.y > 0.7) {
    star_color = mix(
      vec3(0.8, 0.9, 1.0),  // Blue-white
      vec3(1.0, 0.9, 0.7),  // Yellow
      (star_hash.y - 0.7) / 0.3
    );
  } else if (star_hash.y > 0.4) {
    star_color = vec3(0.9, 0.95, 1.0);  // Cool white
  }
  
  // Make stars visible in rim areas and through atmosphere
  float star_visibility = rim_factor_space * 1.5 + 0.3;
  if (!is_moon) {
    // Stars visible through atmosphere on dark side
    float night_side = 1.0 - max(dot(nrm, l_sun), 0.0);
    star_visibility += night_side * 0.5;
  }
  float star_field = star_brightness * star_visibility * 1.5;
  lit += star_color * star_field;
  
  // Combine all effects
  lit += nebula_color;
  
  color = clamp(lit, 0.0, 1.0);
}
