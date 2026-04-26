/*{
  "DESCRIPTION": "Procedural cat — head and optional body, all drawn with signed distance functions.",
  "CREDIT": "yr0-lab",
  "ISFVSN": "2",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    { "NAME": "fur_color",    "TYPE": "color", "DEFAULT": [0.78, 0.58, 0.38, 1.0] },
    { "NAME": "eye_color",    "TYPE": "color", "DEFAULT": [0.18, 0.72, 0.28, 1.0] },
    { "NAME": "bg_color",     "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.12, 1.0] },
    { "NAME": "pupil_dilation","TYPE": "float","DEFAULT": 0.4,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "animate",      "TYPE": "bool",  "DEFAULT": true },
    { "NAME": "blink_rate",   "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2, "MAX": 4.0 },
    { "NAME": "bob_amp",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "ear_amp",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "whisker_amp",  "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "tail_amp",     "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "show_body",    "TYPE": "bool",  "DEFAULT": false },
    { "NAME": "neck_gap",     "TYPE": "float", "DEFAULT": 0.04, "MIN": -0.20, "MAX": 0.28 },
    { "NAME": "body_scale",   "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.45,"MAX": 0.90 }
  ]
}*/

// ------------------------------------------------------------------ SDFs

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

// Quadratic Bezier SDF — returns distance to the curve (unsigned)
float sdBezier(vec2 pos, vec2 A, vec2 B, vec2 C) {
    vec2 a=B-A, b=A-2.0*B+C, c=a*2.0, d=A-pos;
    float kk=1.0/dot(b,b);
    float kx=kk*dot(a,b);
    float ky=kk*(2.0*dot(a,a)+dot(d,b))/3.0;
    float kz=kk*dot(d,a);
    float p=ky-kx*kx, p3=p*p*p;
    float q=kx*(2.0*kx*kx-3.0*ky)+kz;
    float h=q*q+4.0*p3;
    float res;
    if (h>=0.0) {
        h=sqrt(h);
        vec2 x=(vec2(h,-h)-q)/2.0;
        vec2 uv2=sign(x)*pow(abs(x),vec2(1.0/3.0));
        float t2=clamp(uv2.x+uv2.y-kx,0.0,1.0);
        vec2 qos=d+(c+b*t2)*t2;
        res=dot(qos,qos);
    } else {
        float z=sqrt(-p);
        float v=acos(q/(p*z*2.0))/3.0;
        float m=cos(v), n=sin(v)*1.732050808;
        vec3 t3=clamp(vec3(m+m,-n-m,n-m)*z-kx,0.0,1.0);
        float da=dot(d+(c+b*t3.x)*t3.x,d+(c+b*t3.x)*t3.x);
        float db=dot(d+(c+b*t3.y)*t3.y,d+(c+b*t3.y)*t3.y);
        res=min(da,db);
    }
    return sqrt(res);
}

// CCW rotation matrix (GLSL column-major)
mat2 rot2(float a) { float c=cos(a),s=sin(a); return mat2(c,s,-s,c); }

float fill(float d, float aa) { return smoothstep(aa,-aa,d); }

// ------------------------------------------------------------------ main

void main() {
    vec2 uv = isf_FragNormCoord.xy * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;
    float aa = 1.5 / min(RENDERSIZE.x, RENDERSIZE.y);

    // Scene scale — zoom out to frame head + body
    float sc = show_body ? body_scale : 1.0;
    uv = uv / sc + vec2(0.0, show_body ? -0.28 : 0.0);

    // ---- Timing ----
    float t = animate ? TIME : 0.0;

    float bp    = mod(t * blink_rate, 5.0);
    float blink = 1.0 - smoothstep(0.0,0.06,bp)*(1.0-smoothstep(0.06,0.22,bp));

    float bobA = bob_amp * 0.016;
    uv.y -= animate ? sin(t*1.1)*bobA : 0.0;

    // ---------------------------------------------------------------- HEAD
    vec2 headP = uv; headP.y *= 1.08;
    float head = sdCircle(headP, 0.38);

    // ---------------------------------------------------------------- EARS
    // Narrower triangles; tip oscillates via pivot rotation around base midpoint
    vec2 earPivL = vec2(-0.205, 0.188);
    vec2 earPivR = vec2( 0.205, 0.188);
    float earAngle = animate ? sin(t*2.1+0.5)*(ear_amp*0.11) : 0.0;

    // Rotate sample point by -angle around pivot → object appears rotated by +angle
    vec2 uvEL = earPivL + rot2(-earAngle) * (uv - earPivL);
    vec2 uvER = earPivR + rot2( earAngle) * (uv - earPivR);

    float earL = sdTriangle(uvEL, vec2(-0.33,0.17), vec2(-0.205,0.46), vec2(-0.10,0.21));
    float earR = sdTriangle(uvER, vec2( 0.10,0.21), vec2( 0.205,0.46), vec2( 0.33,0.17));
    float ears = min(earL, earR);

    float innerEarL = sdTriangle(uvEL, vec2(-0.29,0.19), vec2(-0.205,0.39), vec2(-0.13,0.22));
    float innerEarR = sdTriangle(uvER, vec2( 0.13,0.22), vec2( 0.205,0.39), vec2( 0.29,0.19));

    // ---------------------------------------------------------------- EYES
    vec2 eyeLPos = vec2(-0.135, 0.04);
    vec2 eyeRPos = vec2( 0.135, 0.04);
    float eR = 0.078;
    float eyeL  = sdCircle(uv - eyeLPos, eR);
    float eyeR2 = sdCircle(uv - eyeRPos, eR);

    float breathe = animate ? 0.5+0.5*sin(t*0.5) : 0.5;
    float slitW = clamp(mix(0.007, eR*0.82, pupil_dilation+breathe*0.14), 0.007, eR*0.88);
    float pupilL = sdBox(uv - eyeLPos, vec2(slitW, eR*0.86));
    float pupilR = sdBox(uv - eyeRPos, vec2(slitW, eR*0.86));

    // Eyelid: at blink=1 (open) center sits 2.3R above eye → fully outside eye circle
    float lidH  = eR * 1.12;
    vec2  lidCL = eyeLPos + vec2(0.0, eR*2.3*blink);
    vec2  lidCR = eyeRPos + vec2(0.0, eR*2.3*blink);
    float lidL  = sdBox(uv - lidCL, vec2(eR*1.18, lidH));
    float lidR2 = sdBox(uv - lidCR, vec2(eR*1.18, lidH));

    vec2  shineOff = vec2(0.022, 0.026);
    float shineL   = sdCircle(uv - eyeLPos - shineOff, 0.017);
    float shineR   = sdCircle(uv - eyeRPos - shineOff, 0.017);

    // ---------------------------------------------------------------- NOSE
    float nose = sdTriangle(uv, vec2(-0.034,-0.072), vec2(0.0,-0.100), vec2(0.034,-0.072));

    // ---------------------------------------------------------------- MOUTH
    float philtrum  = sdSegment(uv, vec2(0.0,-0.100), vec2(0.0,-0.130));
    float mouthL2   = sdSegment(uv, vec2(0.0,-0.130), vec2(-0.068,-0.158));
    float mouthLtip = sdSegment(uv, vec2(-0.068,-0.158), vec2(-0.105,-0.145));
    float mouthR2   = sdSegment(uv, vec2(0.0,-0.130), vec2( 0.068,-0.158));
    float mouthRtip = sdSegment(uv, vec2( 0.068,-0.158), vec2( 0.105,-0.145));
    float mouth     = min(min(philtrum,mouthL2), min(mouthLtip,min(mouthR2,mouthRtip)));

    // ---------------------------------------------------------------- WHISKERS
    vec2  wRootL = vec2(-0.048,-0.096);
    vec2  wRootR = vec2( 0.048,-0.096);
    float wSway  = animate ? sin(t*1.65)*(whisker_amp*0.10) : 0.0;
    vec2  uvWL   = wRootL + rot2(-wSway) * (uv - wRootL);
    vec2  uvWR   = wRootR + rot2( wSway) * (uv - wRootR);
    float wL1=sdSegment(uvWL, wRootL, wRootL+vec2(-0.335, 0.032));
    float wL2=sdSegment(uvWL, wRootL, wRootL+vec2(-0.335, 0.000));
    float wL3=sdSegment(uvWL, wRootL, wRootL+vec2(-0.335,-0.040));
    float wR1=sdSegment(uvWR, wRootR, wRootR+vec2( 0.335, 0.032));
    float wR2=sdSegment(uvWR, wRootR, wRootR+vec2( 0.335, 0.000));
    float wR3=sdSegment(uvWR, wRootR, wRootR+vec2( 0.335,-0.040));
    float whiskers = min(min(wL1,wL2), min(wL3, min(min(wR1,wR2),wR3)));

    // ---------------------------------------------------------------- FOREHEAD STRIPES
    float strM=sdSegment(uv, vec2( 0.000,0.22), vec2( 0.000,0.36));
    float strL=sdSegment(uv, vec2(-0.095,0.20), vec2(-0.065,0.34));
    float strR=sdSegment(uv, vec2( 0.095,0.20), vec2( 0.065,0.34));
    float stripes = min(strM, min(strL, strR));

    // ---------------------------------------------------------------- BODY
    // bY: body center Y.  -0.352 = 0.38/1.08 (head ellipse y-radius = head bottom)
    // neck_gap=0 → body top exactly meets head bottom, no gap, no neck box
    float bY = -0.632 - neck_gap;

    // Neck — invisible at gap<=0 (head overlaps body), grows proportionally above 0
    float neckHalf = max(0.0, neck_gap * 0.5);
    float neckCY   = (-0.352 + bY + 0.28) * 0.5;
    float neck_sdf = sdBox(uv - vec2(0.0, neckCY), vec2(0.108, neckHalf));

    // Body oval — slightly wider than tall
    vec2 bodyP = uv - vec2(0.0, bY);
    bodyP.x /= 0.88;
    float body_sdf = sdCircle(bodyP, 0.28);

    // Tail — two chained quadratic Beziers → smooth C-curve
    // Pivots at the right flank so the whole tail sways as one piece
    float tSway  = animate ? sin(t*0.65)*(tail_amp*0.32) : 0.0;
    vec2  tailPiv = vec2(0.24, bY);
    vec2  uvT     = tailPiv + rot2(-tSway) * (uv - tailPiv);

    vec2 tA = vec2(0.24,  bY);
    vec2 tB = vec2(0.58,  bY - 0.08);  // control: outward-right
    vec2 tC = vec2(0.50,  bY - 0.30);  // mid-arc
    vec2 tD = vec2(0.36,  bY - 0.44);  // control: curls inward
    vec2 tE = vec2(0.18,  bY - 0.28);  // tip

    float tail1   = sdBezier(uvT, tA, tB, tC) - 0.033;  // thicker at root
    float tail2   = sdBezier(uvT, tC, tD, tE) - 0.024;  // tapers toward tip
    float tail_sdf = min(tail1, tail2);
    float tailTip  = sdCircle(uvT - tE, 0.024);          // smooth rounded tip

    // Front paws — flattened ovals at base of body
    vec2 pawLP = uv - vec2(-0.13, bY - 0.31); pawLP.y *= 0.65;
    float pawL2 = sdCircle(pawLP, 0.092);
    vec2 pawRP = uv - vec2( 0.13, bY - 0.31); pawRP.y *= 0.65;
    float pawR3 = sdCircle(pawRP, 0.092);

    // Belly highlight
    vec2 bellyP = uv - vec2(0.0, bY - 0.04); bellyP.y *= 1.25;
    float belly = sdCircle(bellyP, 0.155);

    // Toe lines — three short dashes per paw
    float toeL = min(min(
        sdSegment(uv, vec2(-0.20, bY-0.33), vec2(-0.20, bY-0.38)),
        sdSegment(uv, vec2(-0.13, bY-0.34), vec2(-0.13, bY-0.39))),
        sdSegment(uv, vec2(-0.06, bY-0.33), vec2(-0.06, bY-0.38)));
    float toeR = min(min(
        sdSegment(uv, vec2( 0.06, bY-0.33), vec2( 0.06, bY-0.38)),
        sdSegment(uv, vec2( 0.13, bY-0.34), vec2( 0.13, bY-0.39))),
        sdSegment(uv, vec2( 0.20, bY-0.33), vec2( 0.20, bY-0.38)));
    float toes = min(toeL, toeR);

    // ---------------------------------------------------------------- COMPOSITING
    vec3 col  = bg_color.rgb;
    vec3 fur  = fur_color.rgb;
    vec3 pink = vec3(0.96, 0.60, 0.68);

    // Body layer — drawn first so head occludes it
    if (show_body) {
        col = mix(col, fur, fill(tail_sdf, aa));
        col = mix(col, fur, fill(tailTip,  aa));
        col = mix(col, fur, fill(neck_sdf, aa));
        col = mix(col, fur, fill(body_sdf, aa));
        col = mix(col, mix(fur,vec3(1.0),0.45), fill(belly,aa)*fill(body_sdf,aa));
        col = mix(col, fur, fill(pawL2, aa));
        col = mix(col, fur, fill(pawR3, aa));
        col = mix(col, fur*0.55, step(toes,0.0045)*(fill(pawL2,aa)+fill(pawR3,aa)));
    }

    // Head + ears
    col = mix(col, fur, fill(min(head,ears), aa));
    col = mix(col, pink, fill(innerEarL, aa));
    col = mix(col, pink, fill(innerEarR, aa));
    col = mix(col, fur*0.55, step(stripes,0.006)*fill(head,aa));

    // Eyes
    col = mix(col, eye_color.rgb, fill(eyeL,  aa));
    col = mix(col, eye_color.rgb, fill(eyeR2, aa));
    col = mix(col, vec3(0.03,0.02,0.04), fill(pupilL,aa)*fill(eyeL, aa)*blink);
    col = mix(col, vec3(0.03,0.02,0.04), fill(pupilR,aa)*fill(eyeR2,aa)*blink);
    col = mix(col, vec3(1.0), fill(shineL,aa)*fill(eyeL, aa)*blink);
    col = mix(col, vec3(1.0), fill(shineR,aa)*fill(eyeR2,aa)*blink);
    col = mix(col, fur, fill(lidL, aa)*fill(eyeL, aa));
    col = mix(col, fur, fill(lidR2,aa)*fill(eyeR2,aa));

    // Nose + mouth
    col = mix(col, vec3(0.90,0.38,0.50), fill(nose, aa));
    col = mix(col, vec3(0.28,0.10,0.14), step(mouth,0.0055)*fill(head,aa));

    // Whiskers — always on top
    col = mix(col, vec3(0.95,0.95,0.98), step(whiskers,0.0028));

    gl_FragColor = vec4(col, 1.0);
}
