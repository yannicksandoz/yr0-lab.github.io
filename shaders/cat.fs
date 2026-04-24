/*{
  "DESCRIPTION": "Procedural cat face — draws a cute animated cat using signed distance functions.",
  "CREDIT": "yr0-lab",
  "ISFVSN": "2",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    {
      "NAME": "fur_color",
      "TYPE": "color",
      "DEFAULT": [0.78, 0.58, 0.38, 1.0]
    },
    {
      "NAME": "eye_color",
      "TYPE": "color",
      "DEFAULT": [0.18, 0.72, 0.28, 1.0]
    },
    {
      "NAME": "bg_color",
      "TYPE": "color",
      "DEFAULT": [0.05, 0.05, 0.12, 1.0]
    },
    {
      "NAME": "pupil_dilation",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "animate",
      "TYPE": "bool",
      "DEFAULT": true
    }
  ]
}*/

// --- SDF primitives ---

float sdCircle(vec2 p, float r) {
    return length(p) - r;
}

float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float sdEllipse(vec2 p, vec2 ab) {
    p = abs(p);
    if (p.x > p.y) { p = p.yx; ab = ab.yx; }
    float l = ab.y * ab.y - ab.x * ab.x;
    float m = ab.x * p.x / l;
    float n = ab.y * p.y / l;
    float m2 = m * m, n2 = n * n;
    float c = (m2 + n2 - 1.0) / 3.0;
    float c3 = c * c * c;
    float q = c3 + m2 * n2 * 2.0;
    float d = c3 + m2 * n2;
    float g = m + m * n2;
    float co;
    if (d < 0.0) {
        float h = acos(q / c3) / 3.0;
        float s = cos(h);
        float t2 = sin(h) * sqrt(3.0);
        float rx = sqrt(-c * (s + t2 + 2.0) + m2);
        float ry = sqrt(-c * (s - t2 + 2.0) + m2);
        co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
    } else {
        float h = 2.0 * m * n * sqrt(d);
        float s = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
        float t2 = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
        float rx = -(s + t2) - c * 4.0 + 2.0 * m2;
        float ry = (s - t2) * sqrt(3.0);
        float rm = sqrt(rx * rx + ry * ry);
        co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
    }
    vec2 r2 = ab * vec2(co, sqrt(1.0 - co * co));
    return length(r2 - p) * sign(p.y - r2.y);
}

float sdTriangle(vec2 p, vec2 a, vec2 b, vec2 c) {
    vec2 e0 = b - a, e1 = c - b, e2 = a - c;
    vec2 v0 = p - a, v1 = p - b, v2 = p - c;
    vec2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    float s = sign(e0.x * e2.y - e0.y * e2.x);
    vec2 d = min(min(
        vec2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
        vec2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
        vec2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    return -sqrt(d.x) * sign(d.y);
}

float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    return length(pa - ba * clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0));
}

// --- Utility ---

float fill(float d, float aa) {
    return smoothstep(aa, -aa, d);
}

// Union (soft blend)
float opUnion(float a, float b) { return min(a, b); }

void main() {
    vec2 uv = isf_FragNormCoord.xy * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;

    float aa = 1.5 / min(RENDERSIZE.x, RENDERSIZE.y);

    // --- Timing ---
    float t = animate ? TIME : 0.0;

    // Blink: quick close every ~5s
    float blinkPhase = mod(t, 5.0);
    float blink = 1.0 - smoothstep(0.0, 0.06, blinkPhase) * (1.0 - smoothstep(0.06, 0.22, blinkPhase));

    // Subtle head bob
    float bob = animate ? sin(t * 1.1) * 0.008 : 0.0;
    uv.y -= bob;

    // === HEAD ===
    // Slightly oval: compress y a little
    vec2 headP = uv;
    headP.y *= 1.08;
    float head = sdCircle(headP, 0.38);

    // === EARS ===
    // Left ear
    float earL = sdTriangle(uv,
        vec2(-0.37, 0.14),
        vec2(-0.22, 0.48),
        vec2(-0.08, 0.19)
    );
    // Right ear (mirrored)
    float earR = sdTriangle(uv,
        vec2( 0.08, 0.19),
        vec2( 0.22, 0.48),
        vec2( 0.37, 0.14)
    );
    float ears = opUnion(earL, earR);

    // Inner ear (pink) — slightly inset triangles
    float innerEarL = sdTriangle(uv,
        vec2(-0.33, 0.16),
        vec2(-0.22, 0.40),
        vec2(-0.11, 0.20)
    );
    float innerEarR = sdTriangle(uv,
        vec2( 0.11, 0.20),
        vec2( 0.22, 0.40),
        vec2( 0.33, 0.16)
    );

    // === EYES ===
    vec2 eyeLPos = vec2(-0.135, 0.04);
    vec2 eyeRPos = vec2( 0.135, 0.04);
    float eyeRadius = 0.078;

    float eyeL = sdCircle(uv - eyeLPos, eyeRadius);
    float eyeR = sdCircle(uv - eyeRPos, eyeRadius);

    // Pupils — vertical slit; width driven by pupil_dilation + animated breathing
    float breathe = animate ? 0.5 + 0.5 * sin(t * 0.5) : 0.5;
    float slitW = mix(0.007, eyeRadius * 0.82, pupil_dilation + breathe * 0.15);
    slitW = clamp(slitW, 0.007, eyeRadius * 0.88);

    float pupilL = sdBox(uv - eyeLPos, vec2(slitW, eyeRadius * 0.86));
    float pupilR = sdBox(uv - eyeRPos, vec2(slitW, eyeRadius * 0.86));

    // Eye lid for blinking
    float lidH = eyeRadius * (1.0 - blink) + 0.002;
    vec2 lidOffsetL = eyeLPos + vec2(0.0, eyeRadius * blink * 0.5);
    vec2 lidOffsetR = eyeRPos + vec2(0.0, eyeRadius * blink * 0.5);
    float lidL = sdBox(uv - lidOffsetL, vec2(eyeRadius * 1.15, lidH));
    float lidR = sdBox(uv - lidOffsetR, vec2(eyeRadius * 1.15, lidH));

    // Eye shine — small ellipse offset up-right
    vec2 shineOff = vec2(0.022, 0.026);
    float shineL = sdCircle(uv - eyeLPos - shineOff, 0.017);
    float shineR = sdCircle(uv - eyeRPos - shineOff, 0.017);

    // === NOSE ===
    float nose = sdTriangle(uv,
        vec2(-0.034, -0.072),
        vec2( 0.000, -0.100),
        vec2( 0.034, -0.072)
    );

    // === MOUTH ===
    // Center divider (philtrum) down from nose
    float philtrum = sdSegment(uv, vec2(0.0, -0.100), vec2(0.0, -0.130));
    // Left curve
    float mouthL = sdSegment(uv, vec2(0.0, -0.130), vec2(-0.068, -0.158));
    float mouthLtip = sdSegment(uv, vec2(-0.068, -0.158), vec2(-0.105, -0.145));
    // Right curve (mirror)
    float mouthR = sdSegment(uv, vec2(0.0, -0.130), vec2( 0.068, -0.158));
    float mouthRtip = sdSegment(uv, vec2( 0.068, -0.158), vec2( 0.105, -0.145));
    float mouth = min(min(philtrum, mouthL), min(mouthLtip, min(mouthR, mouthRtip)));

    // === WHISKERS ===
    float wT = 0.0028; // whisker half-thickness for step test
    // Left (fan out)
    float wL1 = sdSegment(uv, vec2(-0.045, -0.087), vec2(-0.38, -0.062));
    float wL2 = sdSegment(uv, vec2(-0.045, -0.096), vec2(-0.38, -0.096));
    float wL3 = sdSegment(uv, vec2(-0.045, -0.106), vec2(-0.38, -0.132));
    // Right
    float wR1 = sdSegment(uv, vec2( 0.045, -0.087), vec2( 0.38, -0.062));
    float wR2 = sdSegment(uv, vec2( 0.045, -0.096), vec2( 0.38, -0.096));
    float wR3 = sdSegment(uv, vec2( 0.045, -0.106), vec2( 0.38, -0.132));
    float whiskers = min(min(wL1, wL2), min(wL3, min(min(wR1, wR2), wR3)));

    // === FOREHEAD STRIPES ===
    float strM  = sdSegment(uv, vec2( 0.000, 0.22), vec2( 0.000, 0.36));
    float strL  = sdSegment(uv, vec2(-0.095, 0.20), vec2(-0.065, 0.34));
    float strR  = sdSegment(uv, vec2( 0.095, 0.20), vec2( 0.065, 0.34));
    float stripes = min(strM, min(strL, strR));

    // === COMPOSITING ===
    vec3 col = bg_color.rgb;

    // Fur body (head + ears)
    float bodyMask = fill(opUnion(head, ears), aa);
    col = mix(col, fur_color.rgb, bodyMask);

    // Inner ear pink
    vec3 pink = vec3(0.96, 0.60, 0.68);
    col = mix(col, pink, fill(innerEarL, aa));
    col = mix(col, pink, fill(innerEarR, aa));

    // Forehead stripes — darker fur
    float stripeMask = step(stripes, 0.006) * fill(head, aa);
    col = mix(col, fur_color.rgb * 0.55, stripeMask);

    // Iris
    col = mix(col, eye_color.rgb, fill(eyeL, aa));
    col = mix(col, eye_color.rgb, fill(eyeR, aa));

    // Pupil slit
    col = mix(col, vec3(0.03, 0.02, 0.04), fill(pupilL, aa) * fill(eyeL, aa) * blink);
    col = mix(col, vec3(0.03, 0.02, 0.04), fill(pupilR, aa) * fill(eyeR, aa) * blink);

    // Eye shine
    col = mix(col, vec3(1.0), fill(shineL, aa) * fill(eyeL, aa) * blink);
    col = mix(col, vec3(1.0), fill(shineR, aa) * fill(eyeR, aa) * blink);

    // Eyelid blink (cover with fur)
    col = mix(col, fur_color.rgb, fill(lidL, aa) * fill(eyeL, aa));
    col = mix(col, fur_color.rgb, fill(lidR, aa) * fill(eyeR, aa));

    // Nose
    col = mix(col, vec3(0.90, 0.38, 0.50), fill(nose, aa));

    // Mouth lines (dark, thin)
    col = mix(col, vec3(0.28, 0.10, 0.14),
        step(mouth, 0.0055) * fill(head, aa));

    // Whiskers (light, on top of everything)
    col = mix(col, vec3(0.95, 0.95, 0.98), step(whiskers, wT));

    gl_FragColor = vec4(col, 1.0);
}
