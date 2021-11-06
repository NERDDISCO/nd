// License: MIT 2021 by NERDDISCO
// Based on the work "Infinite Mandala" by Liam Egan
// https://codepen.io/shubniggurath/pen/Qooevz

/*{
    "DESCRIPTION": "nd-infinite-mandala",
    "CREDIT": "NERDDISCO",
    "ISFVSN": "2",
    "CATEGORIES": [
        "mandala"
    ],
    "INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
        {
            "NAME": "fftImage",
            "TYPE": "audioFFT"
        },
        {
            "NAME": "progress",
            "TYPE": "float",
            "DEFAULT": 4.0,
            "MIN": 0.0,
            "MAX": 100.0
        },
        {
            "NAME": "progress_auto",
            "TYPE": "bool",
            "DEFAULT": 1.0
        },
        {
            "NAME": "seed",
            "TYPE": "float",
            "DEFAULT": 1.2727,
            "MIN": 0.0,
            "MAX": 10.0
        },
        {
            "NAME": "seed2",
            "TYPE": "float",
            "DEFAULT": 5.27,
            "MIN": 0.0,
            "MAX": 50.0
        },
        {
            "NAME": "seed_sub_detail",
            "TYPE": "float",
            "DEFAULT": 45.0,
            "MIN": 0.0,
            "MAX": 100.0
        },
        {
            "NAME": "seed_sub_rotation",
            "TYPE": "float",
            "DEFAULT": 0.89,
            "MIN": 0.0,
            "MAX": 10.0
        },
        {
            "NAME": "circle_size",
            "TYPE": "float",
            "DEFAULT": 0.25,
            "MIN": 0.0,
            "MAX": 10.0
        },
        {
            "NAME": "rotation",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 10.0
        },
        {
            "NAME": "rotation_auto",
            "TYPE": "bool",
            "DEFAULT": 0.0
        },
        {
            "NAME": "rotation_auto_speed",
            "TYPE": "float",
            "DEFAULT": 1.0,
            "MIN": 0.0,
            "MAX": 10.0
        },
        {
            "NAME": "rotation_slice",
            "TYPE": "float",
            "DEFAULT": 0.0,
            "MIN": 0.0,
            "MAX": 20.0
        },
        {
            "NAME": "rainbow",
            "TYPE": "bool",
            "DEFAULT": 1.0
        },
        {
            "NAME": "rainbow_intensity",
            "TYPE": "float",
            "DEFAULT": 4.0,
            "MIN": 0.0,
            "MAX": 20.0
        },
        {
            "NAME": "audio_activated",
            "TYPE": "bool",
            "DEFAULT": 1.0
        },
		{
			"NAME": "audio_minRange",
			"TYPE": "float",
            "DEFAULT": 0.32,
			"MIN": 0.0,
			"MAX": 1.0
		},
		{
			"NAME": "audio_maxRange",
			"TYPE": "float",
            "DEFAULT": 0.44,
			"MIN": 0.0,
			"MAX": 1.0
		}
    ]
}*/

#define TAU 6.2831853071795864769252867665590

mat2 rotate2d(in float radians){
    float c = cos(radians);
    float s = sin(radians);
    return mat2(c, -s, s, c);
}

vec2 rotate(in vec2 st, in float radians, in vec2 center) {
  return rotate2d(radians) * (st - center) + center;
}

float aastep(float threshold, float value) {
    return step(threshold, value);
}

float fill(float x, float size) {
    return 1.0 - aastep(size, x);
}

vec2 scale(vec2 st, vec2 s) {
    return (st-.5)*s+.5;
}

float circleSDF(in vec2 st, in vec2 center) {
    return length(st - center) * 2.;
}

float circle(vec2 st, float size, in vec2 center) {
    return fill(circleSDF(st, center), size);
}

float flowerSDF(vec2 st, int N) {
    st = st * 2.0 - 1.0;
    float r = length(st) * 2.0;
    float a = atan(st.y, st.x);
    float v = float(N) * 0.5;
    return 1.0 - (abs(cos(a * v)) *  0.5 + 0.5) / r;
}

float starSDF(in vec2 st, in int V, in float s) {
    st = st * 4. - 2.;
    float a = atan(st.y, st.x) / TAU;
    float seg = a * float(V);
    a = ((floor(seg) + .5) / float(V) +
        mix(s, -s, step(.5, fract(seg))))
        * TAU;
    return abs(dot(vec2(cos(a), sin(a)),
                   st));
}

float flower(in vec2 st, int sides, float starSize, bool multiply) {
    float color = fill(flowerSDF(st, sides), .45);
    color -= step(.95, starSDF(rotate(st, 0.628, vec2(.5)), sides, starSize));

    color *= multiply ? fill(flowerSDF(st, sides), .05) : 1.0;

    return color;
}

vec2 diagonalhash2(vec2 p) {
    return fract(vec2(sin((p.x + p.y) * 15.543) * 73964.686, sin((p.x + p.y) * 55.8543) * 28560.986));
}

vec3 palette(in float t, in vec3 a, in vec3 b, in vec3 c, in vec3 d) { 
    return a + b * cos(6.28318 * (c * t + d)); 
}
  
vec3 getColour(float d) {
    vec3 c = vec3(0.15);
    if( d <= .25 ) {
      // #F44336
      c = vec3(0.9568627450980393, 0.2627450980392157, 0.21176470588235294);
      // c = vec3( 0.843, 0.149, 0.239 );
    } else if( d <= .5 ) {
      // #1E88E5
      c = vec3(0.11764705882352941, 0.5333333333333333, 0.8980392156862745);
      // c = vec3( 0.18, 0.161, 0.306 );
    } else if( d <= .75 ) {
      // #FDD835
      c = vec3(0.9921568627450981, 0.8470588235294118, 0.20784313725490197);
      // c = vec3( 0.773, 0.847, 0.427 );
    }

    if (rainbow) {
        c = vec3(palette(
		d * rainbow_intensity,
		vec3(0.65, 0.95, 0.85), // brightness
		vec3(1.), // contrast
		vec3(1.), // osc
		vec3(0.11, 0.33, 0.66) // phase
	));
    }
    
    return c;
}

vec4 getFft(in sampler2D fftImage, in float minRange, in float maxRange) {
	vec2 loc = isf_FragNormCoord;
	loc.x = loc.x * abs(maxRange - minRange) + minRange;
	vec4 fft = IMG_NORM_PIXEL(fftImage, vec2(loc.x, 0.0));

	return fft;
}
  
vec3 pattern(vec2 uv, vec2 m, float fft) {
    vec3 pattern = vec3(.0);
    vec2 grid = floor(uv);
    vec2 subuv = fract(uv);
    vec2 subpatternuv = fract(uv);
    vec2 rand = diagonalhash2(grid);
    vec2 rand2 = diagonalhash2(grid + seed2 / 0.001);
    float shade = 0.;
    float df;

    float circle1 = .0;
    float flower1 = .0;
    
    float s = sin(seed_sub_rotation);
    float c = cos(seed_sub_rotation);
    subuv *= mat2(c, -s, s, c);

    // Sub patterns
    if (rand.x <= .25 ) {
        df = subuv.x - subuv.y; 
        circle1 = circle(subpatternuv, circle_size + circle_size * fft, vec2(.5));
    } else if (rand.x <= .5) {
        df = 1. - subuv.y - subuv.x;
        flower1 = flower(subpatternuv, 5, .1, false);
    } else if (rand.x <= .75) {
        df = subuv.y - subuv.x + fft;
        vec2 scaled = scale(subpatternuv, vec2(1.0 - fft));
        flower1 = flower(scaled, 11, .05, true);
    } else if (rand.x <= 1.) {
        df = subuv.y - 1. + subuv.x;
        circle1 = circle(subpatternuv, circle_size + circle_size * fft, vec2(.5));
    }

    // Used to draw the sub patterns
    shade = sin(df * floor(seed_sub_detail * rand.x * rand.y));
    shade += sin(df * rand.x * rand.y);
    float aa = rand.x * rand.y * .001;
    shade = smoothstep(.0, aa, shade);
    
    float mouseMask = smoothstep(.1, .6, length(m));
    vec3 c1 = mix(getColour(rand.x), getColour(rand2.x), mouseMask);
    vec3 c2 = mix(getColour(rand.y), getColour(rand2.y), mouseMask);

    pattern = mix(c1, c2, shade);

    pattern = mix(pattern, getColour(rand.x - circle_size), circle1);

    pattern = mix(pattern, getColour(rand2.x * .1), flower1);
    
    return pattern;
}

void main() {
    vec2 uv = gl_FragCoord.xy + gl_FragCoord.xy - floor(RENDERSIZE.xy);
    float _time = progress_auto ? TIME * 0.2 + progress : progress;

    vec4 fft = getFft(fftImage, audio_minRange, audio_maxRange);
    float fftX = 0.0;

    if (audio_activated) {
        fftX = fft.x;
    }

    // Actual rotation
    uv = rotate(uv, rotation_auto ? (rotation + TIME) * rotation_auto_speed : rotation, vec2(.5));
    uv = vec2(log(length(uv) / RENDERSIZE.y) - 2.3 * _time, atan(uv.y, uv.x) + _time * .5);

    // uv *= 4.;
    // float s = sin(1.);
    // float c = cos(1.);
    // uv = 1.911*uv*mat2(c,-1,1,1);
    uv = seed * uv * mat2(1., -1, .9, 1);
    // uv += vec2(TIME, 0.).yx;

    // Slice it up with some rotation
    uv = rotate(uv, rotation_slice, vec2(.5));

    
    gl_FragColor = texture2D(inputImage, uv + diagonalhash2(uv)) * .2;

    vec2 u_mouse = vec2(0, 0);
    vec2 m = u_mouse - (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / min(RENDERSIZE.y, RENDERSIZE.x);
    
    vec3 colour = pattern(uv, vec2(.6, .3), fftX);

    gl_FragColor += vec4(colour,1.0);
}
