/*{
  "DESCRIPTION": "Procedural cat — head (+ optional body) drawn with signed distance functions.",
  "CREDIT": "yr0-lab",
  "ISFVSN": "2",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    { "NAME": "fur_color",      "TYPE": "color", "DEFAULT": [0.78, 0.58, 0.38, 1.0] },
    { "NAME": "eye_color",      "TYPE": "color", "DEFAULT": [0.18, 0.72, 0.28, 1.0] },
    { "NAME": "bg_color",       "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.12, 1.0] },
    { "NAME": "pupil_dilation", "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "animate",        "TYPE": "bool",  "DEFAULT": true },
    { "NAME": "show_body",      "TYPE": "bool",  "DEFAULT": false },
    { "NAME": "body_scale",     "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.45, "MAX": 0.90 }
  ]
}*/

// --- SDF primitives ---

float sdCircle(vec2 p, float r) { return length(p) - r; }

float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float sdTriangle(vec2 p, vec2 a, vec2 b, vec2 c) {
    vec2 e0=b-a, e1=c-b, e2=a-c;
    vec2 v0=p-a, v1=p-b, v2=p-c;
    vec2 pq0=v0-e0*clamp(dot(v0,e0)/dot(e0,e0),0.0,1.0);
    vec2 pq1=v1-e1*clamp(dot(v1,e1)/dot(e1,e1),0.0,1.0);
    vec2 pq2=v2-e2*clamp(dot(v2,e2)/dot(e2,e2),0.0,1.0);
    float s=sign(e0.x*e2.y-e0.y*e2.x);
    vec2 d=min(min(vec2(dot(pq0,pq0),s*(v0.x*e0.y-v0.y*e0.x)),
                   vec2(dot(pq1,pq1),s*(v1.x*e1.y-v1.y*e1.x))),
                   vec2(dot(pq2,pq2),s*(v2.x*e2.y-v2.y*e2.x)));
    return -sqrt(d.x)*sign(d.y);
}

float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa=p-a, ba=b-a;
    return length(pa - ba*clamp(dot(pa,ba)/dot(ba,ba),0.0,1.0));
}

// CCW rotation matrix (column-major GLSL)
mat2 rot2(float a) { float c=cos(a),s=sin(a); return mat2(c,s,-s,c); }

float fill(float d, float aa) { return smoothstep(aa,-aa,d); }

void main() {
    vec2 uv = isf_FragNormCoord.xy * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;
    float aa = 1.5 / min(RENDERSIZE.x, RENDERSIZE.y);

    // --- Scene scale: zoom out when body is visible ---
    float sc = show_body ? body_scale : 1.0;
    // uv/sc shifts to scene coords; -0.28 offset frames head in upper half when body shown
    uv = uv / sc + vec2(0.0, show_body ? -0.28 : 0.0);

    // --- Timing ---
    float t = animate ? TIME : 0.0;

    // Blink: 0 = fully closed, 1 = fully open
    float bp = mod(t, 5.0);
    float blink = 1.0 - smoothstep(0.0,0.06,bp)*(1.0-smoothstep(0.06,0.22,bp));

    // Head bob
    uv.y -= animate ? sin(t*1.1)*0.008 : 0.0;

    // =========================================================
    // HEAD
    // =========================================================
    vec2 headP = uv; headP.y *= 1.08;
    float head = sdCircle(headP, 0.38);

    // =========================================================
    // EARS  — narrower triangles, tip animated via pivot rotation
    // =========================================================
    // Base midpoints (pivot for the rotation)
    vec2 earPivL = vec2(-0.205, 0.188);
    vec2 earPivR = vec2( 0.205, 0.188);

    // Slow oscillating twitch; ears converge/diverge naturally
    float earAngle = animate ? sin(t*2.1+0.5)*0.055 : 0.0;

    // Rotate sample point CW by earAngle around pivot → object appears rotated CCW
    vec2 uvEL = earPivL + rot2(-earAngle) * (uv - earPivL);
    vec2 uvER = earPivR + rot2( earAngle) * (uv - earPivR); // mirror direction

    float earL = sdTriangle(uvEL, vec2(-0.33,0.17), vec2(-0.205,0.46), vec2(-0.10,0.21));
    float earR = sdTriangle(uvER, vec2( 0.10,0.21), vec2( 0.205,0.46), vec2( 0.33,0.17));
    float ears = min(earL, earR);

    float innerEarL = sdTriangle(uvEL, vec2(-0.29,0.19), vec2(-0.205,0.39), vec2(-0.13,0.22));
    float innerEarR = sdTriangle(uvER, vec2( 0.13,0.22), vec2( 0.205,0.39), vec2( 0.29,0.19));

    // =========================================================
    // EYES
    // =========================================================
    vec2 eyeLPos = vec2(-0.135, 0.04);
    vec2 eyeRPos = vec2( 0.135, 0.04);
    float eyeR_ = 0.078;

    float eyeL = sdCircle(uv - eyeLPos, eyeR_);
    float eyeR = sdCircle(uv - eyeRPos, eyeR_);

    // Pupil slit — width breathes slowly
    float breathe = animate ? 0.5+0.5*sin(t*0.5) : 0.5;
    float slitW = clamp(mix(0.007, eyeR_*0.82, pupil_dilation+breathe*0.14), 0.007, eyeR_*0.88);
    float pupilL = sdBox(uv - eyeLPos, vec2(slitW, eyeR_*0.86));
    float pupilR = sdBox(uv - eyeRPos, vec2(slitW, eyeR_*0.86));

    // Eyelid — slides ABOVE the eye circle when blink=1 (open), sweeps down when blink=0 (closed)
    // Center travels from eyePos (blink=0, covering eye) to eyePos+(0,2.3R) (blink=1, fully above eye)
    float lidH = eyeR_ * 1.12;
    vec2 lidCL = eyeLPos + vec2(0.0, eyeR_ * 2.3 * blink);
    vec2 lidCR = eyeRPos + vec2(0.0, eyeR_ * 2.3 * blink);
    float lidL = sdBox(uv - lidCL, vec2(eyeR_*1.18, lidH));
    float lidR = sdBox(uv - lidCR, vec2(eyeR_*1.18, lidH));

    // Eye shine
    vec2 shineOff = vec2(0.022, 0.026);
    float shineL = sdCircle(uv - eyeLPos - shineOff, 0.017);
    float shineR = sdCircle(uv - eyeRPos - shineOff, 0.017);

    // =========================================================
    // NOSE
    // =========================================================
    float nose = sdTriangle(uv, vec2(-0.034,-0.072), vec2(0.0,-0.100), vec2(0.034,-0.072));

    // =========================================================
    // MOUTH
    // =========================================================
    float philtrum  = sdSegment(uv, vec2(0.0,-0.100), vec2(0.0,-0.130));
    float mouthL    = sdSegment(uv, vec2(0.0,-0.130), vec2(-0.068,-0.158));
    float mouthLtip = sdSegment(uv, vec2(-0.068,-0.158), vec2(-0.105,-0.145));
    float mouthR    = sdSegment(uv, vec2(0.0,-0.130), vec2( 0.068,-0.158));
    float mouthRtip = sdSegment(uv, vec2( 0.068,-0.158), vec2( 0.105,-0.145));
    float mouth = min(min(philtrum,mouthL), min(mouthLtip,min(mouthR,mouthRtip)));

    // =========================================================
    // WHISKERS — pivot-based rotation sway (no floating translation)
    // =========================================================
    vec2 wRootL = vec2(-0.048, -0.096);
    vec2 wRootR = vec2( 0.048, -0.096);
    float wSway = animate ? sin(t*1.65)*0.052 : 0.0;

    // Inverse-rotate sample point so whiskers appear to rotate around their root
    vec2 uvWL = wRootL + rot2(-wSway) * (uv - wRootL);
    vec2 uvWR = wRootR + rot2( wSway) * (uv - wRootR); // mirror sway

    float wL1 = sdSegment(uvWL, wRootL, wRootL + vec2(-0.335, 0.032));
    float wL2 = sdSegment(uvWL, wRootL, wRootL + vec2(-0.335, 0.000));
    float wL3 = sdSegment(uvWL, wRootL, wRootL + vec2(-0.335,-0.040));
    float wR1 = sdSegment(uvWR, wRootR, wRootR + vec2( 0.335, 0.032));
    float wR2 = sdSegment(uvWR, wRootR, wRootR + vec2( 0.335, 0.000));
    float wR3 = sdSegment(uvWR, wRootR, wRootR + vec2( 0.335,-0.040));
    float whiskers = min(min(wL1,wL2), min(wL3, min(min(wR1,wR2),wR3)));

    // =========================================================
    // FOREHEAD STRIPES
    // =========================================================
    float strM = sdSegment(uv, vec2( 0.000,0.22), vec2( 0.000,0.36));
    float strL = sdSegment(uv, vec2(-0.095,0.20), vec2(-0.065,0.34));
    float strR = sdSegment(uv, vec2( 0.095,0.20), vec2( 0.065,0.34));
    float stripes = min(strM, min(strL, strR));

    // =========================================================
    // BODY (only when show_body is true)
    // =========================================================

    // Body — slightly wider oval below head
    vec2 bodyP = uv - vec2(0.0, -0.70);
    bodyP.x /= 0.88;
    float body_sdf = sdCircle(bodyP, 0.28);

    // Tail — pivots at right flank, sweeps in a gentle arc
    float tailSway = animate ? sin(t*0.65)*0.18 : 0.0;
    vec2 tailPiv = vec2(0.23, -0.70);
    vec2 uvT = tailPiv + rot2(-tailSway) * (uv - tailPiv);
    float tS1 = sdSegment(uvT, vec2(0.23,-0.70), vec2(0.46,-0.52));
    float tS2 = sdSegment(uvT, vec2(0.46,-0.52), vec2(0.58,-0.30));
    float tS3 = sdSegment(uvT, vec2(0.58,-0.30), vec2(0.54,-0.12));
    float tS4 = sdSegment(uvT, vec2(0.54,-0.12), vec2(0.40,-0.07));
    float tailLine = min(min(tS1,tS2), min(tS3,tS4));
    float tailTip  = sdCircle(uvT - vec2(0.40,-0.07), 0.044);

    // Front paws — flattened circles at bottom of body
    vec2 pawLP = uv - vec2(-0.13,-0.94); pawLP.y *= 0.65;
    float pawL_sdf = sdCircle(pawLP, 0.092);
    vec2 pawRP = uv - vec2( 0.13,-0.94); pawRP.y *= 0.65;
    float pawR_sdf = sdCircle(pawRP, 0.092);

    // Belly highlight — lighter oval in center of body
    vec2 bellyP = uv - vec2(0.0,-0.74); bellyP.y *= 1.25;
    float belly = sdCircle(bellyP, 0.155);

    // Toe lines on each paw (three short segments)
    float toeLP1 = sdSegment(uv, vec2(-0.20,-0.96), vec2(-0.20,-1.00));
    float toeLP2 = sdSegment(uv, vec2(-0.13,-0.97), vec2(-0.13,-1.02));
    float toeLP3 = sdSegment(uv, vec2(-0.06,-0.96), vec2(-0.06,-1.00));
    float toeRP1 = sdSegment(uv, vec2( 0.06,-0.96), vec2( 0.06,-1.00));
    float toeRP2 = sdSegment(uv, vec2( 0.13,-0.97), vec2( 0.13,-1.02));
    float toeRP3 = sdSegment(uv, vec2( 0.20,-0.96), vec2( 0.20,-1.00));
    float toes = min(min(toeLP1,toeLP2), min(toeLP3, min(min(toeRP1,toeRP2),toeRP3)));

    // =========================================================
    // COMPOSITING
    // =========================================================
    vec3 col = bg_color.rgb;
    vec3 fur  = fur_color.rgb;
    vec3 pink = vec3(0.96, 0.60, 0.68);

    // --- Body layer (behind head) ---
    if (show_body) {
        col = mix(col, fur, step(tailLine, 0.030));
        col = mix(col, fur, fill(tailTip,  aa));
        col = mix(col, fur, fill(body_sdf, aa));
        col = mix(col, mix(fur, vec3(1.0), 0.45), fill(belly, aa) * fill(body_sdf, aa));
        col = mix(col, fur, fill(pawL_sdf, aa));
        col = mix(col, fur, fill(pawR_sdf, aa));
        col = mix(col, fur*0.55, step(toes, 0.0045) * (fill(pawL_sdf,aa)+fill(pawR_sdf,aa)));
    }

    // --- Head + ears ---
    col = mix(col, fur, fill(min(head, ears), aa));

    // Inner ear
    col = mix(col, pink, fill(innerEarL, aa));
    col = mix(col, pink, fill(innerEarR, aa));

    // Forehead stripes
    col = mix(col, fur*0.55, step(stripes, 0.006) * fill(head, aa));

    // Iris
    col = mix(col, eye_color.rgb, fill(eyeL, aa));
    col = mix(col, eye_color.rgb, fill(eyeR, aa));

    // Pupils
    col = mix(col, vec3(0.03,0.02,0.04), fill(pupilL,aa)*fill(eyeL,aa)*blink);
    col = mix(col, vec3(0.03,0.02,0.04), fill(pupilR,aa)*fill(eyeR,aa)*blink);

    // Shine
    col = mix(col, vec3(1.0), fill(shineL,aa)*fill(eyeL,aa)*blink);
    col = mix(col, vec3(1.0), fill(shineR,aa)*fill(eyeR,aa)*blink);

    // Eyelid sweep (drawn over iris, clipped to eye circle)
    col = mix(col, fur, fill(lidL,aa) * fill(eyeL,aa));
    col = mix(col, fur, fill(lidR,aa) * fill(eyeR,aa));

    // Nose
    col = mix(col, vec3(0.90,0.38,0.50), fill(nose, aa));

    // Mouth
    col = mix(col, vec3(0.28,0.10,0.14), step(mouth, 0.0055) * fill(head, aa));

    // Whiskers (always on top)
    col = mix(col, vec3(0.95,0.95,0.98), step(whiskers, 0.0028));

    gl_FragColor = vec4(col, 1.0);
}
