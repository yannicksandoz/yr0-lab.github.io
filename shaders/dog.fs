/*{
  "DESCRIPTION": "Procedural dog — head and body with animation controls.",
  "CREDIT": "yr0-lab",
  "ISFVSN": "2",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    { "NAME": "fur_color",     "TYPE": "color", "DEFAULT": [0.82, 0.62, 0.30, 1.0] },
    { "NAME": "eye_color",     "TYPE": "color", "DEFAULT": [0.35, 0.22, 0.08, 1.0] },
    { "NAME": "bg_color",      "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.12, 1.0] },
    { "NAME": "outline_color", "TYPE": "color", "DEFAULT": [0.28, 0.16, 0.06, 1.0] },
    { "NAME": "outline_width", "TYPE": "float", "DEFAULT": 0.013, "MIN": 0.0,   "MAX": 0.04  },
    { "NAME": "pupil_dilation","TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "mouth_open",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "animate",       "TYPE": "bool",  "DEFAULT": true   },
    { "NAME": "blink_rate",    "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.2,   "MAX": 4.0  },
    { "NAME": "blink_t",       "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "bob_amp",       "TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "bob_t",         "TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "ear_amp",       "TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "ear_t",         "TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "tail_amp",      "TYPE": "float", "DEFAULT": 0.5,   "MIN": 0.0,   "MAX": 3.0  },
    { "NAME": "tail_t",        "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "tail_length",   "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.2,   "MAX": 2.5  },
    { "NAME": "show_body",     "TYPE": "bool",  "DEFAULT": false  },
    { "NAME": "neck_gap",      "TYPE": "float", "DEFAULT": -0.05, "MIN": -0.20, "MAX": 0.28 },
    { "NAME": "body_scale",    "TYPE": "float", "DEFAULT": 0.65,  "MIN": 0.45,  "MAX": 0.90 },
    { "NAME": "paw_f_radius",  "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 0.35 },
    { "NAME": "paw_f_t",       "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "paw_f_spread",  "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.15, "MAX": 0.30 },
    { "NAME": "paw_f_l_x",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_f_l_y",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_f_r_x",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_f_r_y",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_f_size",    "TYPE": "float", "DEFAULT": 0.098, "MIN": 0.04,  "MAX": 0.18 },
    { "NAME": "paw_b_radius",  "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 0.35 },
    { "NAME": "paw_b_t",       "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0  },
    { "NAME": "paw_b_spread",  "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.15, "MAX": 0.30 },
    { "NAME": "paw_b_x",       "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_b_y",       "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.40, "MAX": 0.40 },
    { "NAME": "paw_b_size",    "TYPE": "float", "DEFAULT": 0.092, "MIN": 0.04,  "MAX": 0.18 },
    { "NAME": "show_bone",     "TYPE": "bool",  "DEFAULT": false  },
    { "NAME": "bone_color",    "TYPE": "color", "DEFAULT": [0.96, 0.93, 0.85, 1.0] },
    { "NAME": "bone_x",        "TYPE": "float", "DEFAULT": 0.0,   "MIN": -1.0,  "MAX": 1.0  },
    { "NAME": "bone_y",        "TYPE": "float", "DEFAULT": 0.0,   "MIN": -0.60, "MAX": 0.60 },
    { "NAME": "bone_pan",      "TYPE": "float", "DEFAULT": 0.0,   "MIN": -1.0,  "MAX": 1.0  },
    { "NAME": "bone_size",     "TYPE": "float", "DEFAULT": 0.06,  "MIN": 0.02,  "MAX": 0.18 },
    { "NAME": "bone_angle",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": -1.0,  "MAX": 1.0  },
    { "NAME": "ear_color",     "TYPE": "color", "DEFAULT": [0.66, 0.44, 0.18, 1.0] },
    { "NAME": "mirror",        "TYPE": "bool",  "DEFAULT": false  }
  ]
}*/

float sdCircle(vec2 p, float r) { return length(p) - r; }

float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa=p-a, ba=b-a;
    return length(pa - ba*clamp(dot(pa,ba)/dot(ba,ba),0.0,1.0));
}

float sdBezier(vec2 pos, vec2 A, vec2 B, vec2 C) {
    vec2 a=B-A, b=A-2.0*B+C, c=a*2.0, d=A-pos;
    float kk=1.0/dot(b,b), kx=kk*dot(a,b);
    float ky=kk*(2.0*dot(a,a)+dot(d,b))/3.0, kz=kk*dot(d,a);
    float p2=ky-kx*kx, p3=p2*p2*p2, q=kx*(2.0*kx*kx-3.0*ky)+kz;
    float h=q*q+4.0*p3, res;
    if (h>=0.0) {
        h=sqrt(h); vec2 x=(vec2(h,-h)-q)/2.0;
        vec2 uv2=sign(x)*pow(abs(x),vec2(1.0/3.0));
        float t2=clamp(uv2.x+uv2.y-kx,0.0,1.0);
        vec2 qos=d+(c+b*t2)*t2; res=dot(qos,qos);
    } else {
        float z=sqrt(-p2), v=acos(q/(p2*z*2.0))/3.0;
        float m=cos(v), n=sin(v)*1.732050808;
        vec3 t3=clamp(vec3(m+m,-n-m,n-m)*z-kx,0.0,1.0);
        float da=dot(d+(c+b*t3.x)*t3.x,d+(c+b*t3.x)*t3.x);
        float db=dot(d+(c+b*t3.y)*t3.y,d+(c+b*t3.y)*t3.y);
        res=min(da,db);
    }
    return sqrt(res);
}

mat2 rot2(float a){ float c=cos(a),s=sin(a); return mat2(c,s,-s,c); }
float fill(float d, float aa){ return smoothstep(aa,-aa,d); }
float smin(float a, float b, float k){ float h=clamp(0.5+0.5*(b-a)/k,0.0,1.0); return mix(b,a,h)-k*h*(1.0-h); }
float sdPad(vec2 p, vec2 c, float r){ vec2 q=p-c; q.y/=0.62; return sdCircle(q,r); }

void main() {
    vec2 uv = isf_FragNormCoord.xy * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;
    float aa = 1.5 / min(RENDERSIZE.x, RENDERSIZE.y);

    float sc = show_body ? body_scale : 1.0;
    uv = uv / sc + vec2(0.0, show_body ? -0.28 : 0.0);

    float t = animate ? TIME : 0.0;

    // ---- BLINK ----
    float bp = mod(t * blink_rate, 5.0);
    float blinkAuto = 1.0 - smoothstep(0.0,0.06,bp)*(1.0-smoothstep(0.06,0.22,bp));
    float blink = animate ? blinkAuto : blink_t;

    // ---- BOB ----
    float bobManual = (bob_t - 0.5) * 2.0 * 0.030;
    float bobAuto   = animate ? sin(t*1.1)*(bob_amp*0.030) : 0.0;
    uv.y -= bobManual + bobAuto;
    if (mirror) uv.x = -uv.x;

    // ======================================================== HEAD + MUZZLE (Cocker: pear skull, salient organic muzzle)
    vec2 headP = uv;
    float _hy = uv.y; // original y for consistent smoothstep
    headP.y *= mix(1.05, 1.48, smoothstep(0.05, -0.22, _hy));
    headP.x *= mix(0.94, 0.84, smoothstep(0.05, -0.22, _hy));
    float head = sdCircle(headP, 0.40);
    // Organic ellipse muzzle — descends below the pear skull
    vec2 muzzleP = uv - vec2(0.0, -0.220); muzzleP.x /= 0.85; muzzleP.y /= 0.65;
    float muzzle = sdCircle(muzzleP, 0.162);
    float headShape = smin(head, muzzle, 0.06);
    // Nasal bridge ridge (visible shadow at top of muzzle)
    float nasalRidge = sdBox(uv - vec2(0.0, -0.130), vec2(0.045, 0.012));
    // Topknot / forelock curl on skull crown
    float topknot = sdCircle(uv - vec2(0.0, 0.355), 0.052);

    // ======================================================== EARS (Cocker: low pivot, very long pendulous, fringed)
    vec2 earPivL = vec2(-0.37, -0.05);
    vec2 earPivR = vec2( 0.37, -0.05);
    float earManual = (ear_t - 0.5) * 2.0 * 0.18;
    float earAuto   = animate ? sin(t*1.1+0.3)*(ear_amp*0.18) : 0.0;
    float earAngle  = earManual + earAuto;
    vec2 uvEL = earPivL + rot2(-earAngle) * (uv - earPivL);
    vec2 uvER = earPivR + rot2( earAngle) * (uv - earPivR);
    // Long teardrop: center lowered so ear hangs well below jaw
    vec2 earLCenter = vec2(-0.36, -0.40);
    vec2 earRCenter = vec2( 0.36, -0.40);
    vec2 earLP = uvEL - earLCenter; earLP.x /= 0.42; earLP.y /= 1.65;
    float angL = atan(earLP.y, earLP.x);
    float fringeL = (angL < 0.0) ? 0.014 * sin(angL * 10.0) : 0.0;
    float earL = sdCircle(earLP, 0.28 + fringeL) - 0.022;
    vec2 earRP = uvER - earRCenter; earRP.x /= 0.42; earRP.y /= 1.65;
    float angR = atan(earRP.y, earRP.x);
    float fringeR = (angR < 0.0) ? 0.014 * sin(angR * 10.0) : 0.0;
    float earR = sdCircle(earRP, 0.28 + fringeR) - 0.022;
    // Inner ear (silky lighter patch, scales with ear)
    vec2 iEarLP = uvEL - earLCenter; iEarLP.x /= 0.28; iEarLP.y /= 1.30;
    float innerEarL = sdCircle(iEarLP, 0.19) - 0.012;
    vec2 iEarRP = uvER - earRCenter; iEarRP.x /= 0.28; iEarRP.y /= 1.30;
    float innerEarR = sdCircle(iEarRP, 0.19) - 0.012;

    // ======================================================== EYES (Cocker: large, round, soulful)
    vec2 eyeLPos = vec2(-0.155, 0.050);
    vec2 eyeRPos = vec2( 0.155, 0.050);
    float eR = 0.084;
    float eyeL  = sdCircle(uv - eyeLPos, eR);
    float eyeR2 = sdCircle(uv - eyeRPos, eR);
    float breathe = animate ? 0.5+0.5*sin(t*0.5) : 0.5;
    float pupRad  = eR * clamp(0.36 + pupil_dilation*0.54 + breathe*0.08, 0.20, 0.88);
    float pupilL  = sdCircle(uv - eyeLPos, pupRad);
    float pupilR2 = sdCircle(uv - eyeRPos, pupRad);
    vec2  lidCL = eyeLPos + vec2(0.0, eR*2.3*blink);
    vec2  lidCR = eyeRPos + vec2(0.0, eR*2.3*blink);
    float lidL  = sdBox(uv - lidCL, vec2(eR*1.18, eR*1.12));
    float lidR2 = sdBox(uv - lidCR, vec2(eR*1.18, eR*1.12));
    float shineL = sdCircle(uv - eyeLPos - vec2(0.022,0.026), 0.018);
    float shineR = sdCircle(uv - eyeRPos - vec2(0.022,0.026), 0.018);
    // Cocker brow ridges: slight arcs above each eye
    float browL  = sdCircle(uv - eyeLPos - vec2(0.0, 0.110), 0.020);
    float browR  = sdCircle(uv - eyeRPos - vec2(0.0, 0.110), 0.020);

    // ======================================================== NOSE (Cocker: wide, prominent)
    vec2 noseP = uv - vec2(0.0, -0.150); noseP.x /= 0.80;
    float nose = sdCircle(noseP, 0.058);
    float noseShine = sdCircle(uv - vec2(-0.020,-0.132), 0.013);

    // ======================================================== MOUTH + TONGUE
    float jawD = mouth_open * 0.06;
    vec2 mMid = vec2(0.0, -0.238);
    vec2 mCL  = mix(vec2(-0.076,-0.234), vec2(-0.092,-0.246+jawD), mouth_open);
    vec2 mCR  = mix(vec2( 0.076,-0.234), vec2( 0.092,-0.246+jawD), mouth_open);
    float mouth = min(sdSegment(uv, mMid, mCL), sdSegment(uv, mMid, mCR));
    float mouthInteriorR = mouth_open * 0.058;
    vec2  mIntP = uv - mMid; mIntP.x /= 0.72;
    float mInterior = sdCircle(mIntP, mouthInteriorR);
    float tongueR = mouth_open * 0.070;
    vec2  tongC   = mMid - vec2(0.0, mouth_open * 0.042);
    vec2  tongP   = uv - tongC; tongP.x /= 0.68;
    float tongue  = sdCircle(tongP, tongueR);

    // ======================================================== BODY
    float bY = -0.632 - neck_gap;
    float neckHalf = max(0.0, neck_gap * 0.5);
    float neck_sdf = sdBox(uv - vec2(0.0,(-0.352+bY+0.28)*0.5), vec2(0.120, neckHalf));
    vec2 bodyP = uv - vec2(0.0, bY); bodyP.x /= 0.78; bodyP.y /= 0.92;
    float body_sdf = sdCircle(bodyP, 0.30);
    // Poitrail saillant entre encolure et pattes
    vec2 chestP = uv - vec2(0.0, bY + 0.22); chestP.x /= 1.10; chestP.y /= 0.70;
    body_sdf = smin(body_sdf, sdCircle(chestP, 0.13), 0.04);

    // ---- TAIL ----
    float tailAngle = tail_t * 6.2832 + (animate ? t * 1.5 : 0.0);
    float tSwayH = tail_amp * 0.55 * cos(tailAngle);
    float tSwayV = tail_amp * 0.15 * sin(tailAngle);
    float tL     = tail_length;
    vec2  tRoot   = vec2(0.20, bY + 0.18);
    vec2  tailPiv = vec2(0.20, bY + 0.18 + tSwayV);
    vec2  uvT     = tailPiv + rot2(-tSwayH) * (uv - tailPiv);
    vec2 tA = tRoot;
    vec2 tB = tRoot + vec2(0.12,  0.28) * tL;
    vec2 tC = tRoot + vec2(0.00,  0.44) * tL;
    float tailW   = 0.036 + 0.012 * tL;
    float tail_sdf = sdBezier(uvT, tA, tB, tC) - tailW;
    float tailTip  = sdCircle(uvT - (2.0*tC - tB), tailW * 1.25);

    // ---- FRONT PAWS ----
    float pfAngle   = paw_f_t * 6.2832 + (animate ? t * 2.0 : 0.0);
    vec2  pfDelta   = paw_f_radius * vec2(cos(pfAngle), sin(pfAngle)*0.65);
    vec2  pfLCenter = vec2(-0.26-paw_f_spread+paw_f_l_x, bY-0.13+paw_f_l_y) + vec2(-pfDelta.x, pfDelta.y);
    vec2  pfRCenter = vec2( 0.26+paw_f_spread+paw_f_r_x, bY-0.13+paw_f_r_y) + pfDelta;
    vec2 pfLP = uv - pfLCenter; pfLP.y *= 0.72; pfLP.x *= 0.82;
    float pfL = sdCircle(pfLP, paw_f_size);
    vec2 pfRP = uv - pfRCenter; pfRP.y *= 0.72; pfRP.x *= 0.82;
    float pfR = sdCircle(pfRP, paw_f_size);
    float ps  = paw_f_size / 0.098;
    float pr  = 0.022 * ps;
    float toeL = min(min(
        sdPad(uv, pfLCenter+vec2(-0.038,-0.046)*ps, pr),
        sdPad(uv, pfLCenter+vec2( 0.000,-0.056)*ps, pr)),
        sdPad(uv, pfLCenter+vec2( 0.038,-0.046)*ps, pr));
    float toeR = min(min(
        sdPad(uv, pfRCenter+vec2(-0.038,-0.046)*ps, pr),
        sdPad(uv, pfRCenter+vec2( 0.000,-0.056)*ps, pr)),
        sdPad(uv, pfRCenter+vec2( 0.038,-0.046)*ps, pr));

    // ---- BACK PAWS ----
    float pbAngle   = paw_b_t * 6.2832 + (animate ? t * 2.5 : 0.0);
    vec2  pbDelta   = paw_b_radius * vec2(cos(pbAngle), sin(pbAngle)*0.55);
    vec2  pbLCenter = vec2(-0.13-paw_b_spread+paw_b_x, bY-0.31+paw_b_y) + vec2(-pbDelta.x, pbDelta.y);
    vec2  pbRCenter = vec2( 0.13+paw_b_spread+paw_b_x, bY-0.31+paw_b_y) + pbDelta;
    vec2 pbLP = uv - pbLCenter; pbLP.y *= 0.65;
    float pbL = sdCircle(pbLP, paw_b_size);
    vec2 pbRP = uv - pbRCenter; pbRP.y *= 0.65;
    float pbR = sdCircle(pbRP, paw_b_size);
    float pbs = paw_b_size / 0.092;
    float pbr = 0.020 * pbs;
    float toeBL = min(min(
        sdPad(uv, pbLCenter+vec2(-0.032,-0.038)*pbs, pbr),
        sdPad(uv, pbLCenter+vec2( 0.000,-0.046)*pbs, pbr)),
        sdPad(uv, pbLCenter+vec2( 0.032,-0.038)*pbs, pbr));
    float toeBR = min(min(
        sdPad(uv, pbRCenter+vec2(-0.032,-0.038)*pbs, pbr),
        sdPad(uv, pbRCenter+vec2( 0.000,-0.046)*pbs, pbr)),
        sdPad(uv, pbRCenter+vec2( 0.032,-0.038)*pbs, pbr));

    // ---- BELLY ----
    vec2 bellyP = uv - vec2(0.0, bY-0.04); bellyP.y *= 1.25;
    float belly = sdCircle(bellyP, 0.155);

    // ---- BONE ----
    float bsn     = bone_size / 0.06;
    float boneHL  = 0.095 * bsn;
    float knobR   = 0.036 * bsn;
    float knobOff = knobR  * 0.72;
    float shaftH  = 0.016 * bsn;
    vec2  boneCenter = vec2(bone_x + bone_pan, bY - 0.36 + bone_y);
    vec2  boneRel    = rot2(bone_angle * 3.14159) * (uv - boneCenter);
    float shaft   = sdBox(boneRel, vec2(boneHL, shaftH));
    float kL1 = sdCircle(boneRel - vec2(-boneHL,  knobOff), knobR);
    float kL2 = sdCircle(boneRel - vec2(-boneHL, -knobOff), knobR);
    float kR1 = sdCircle(boneRel - vec2( boneHL,  knobOff), knobR);
    float kR2 = sdCircle(boneRel - vec2( boneHL, -knobOff), knobR);
    float bone_sdf = min(min(shaft, min(kL1,kL2)), min(kR1,kR2));

    // ======================================================== COMPOSITING
    vec3 col = bg_color.rgb;
    vec3 fur = fur_color.rgb;
    float outW = outline_width;
    vec3  outC = outline_color.rgb;

    if (show_body) {
        col = mix(col, outC, fill(tail_sdf - outW, aa));
        col = mix(col, fur,  fill(tail_sdf, aa));
        col = mix(col, outC, fill(tailTip - outW, aa));
        col = mix(col, fur,  fill(tailTip, aa));
        col = mix(col, outC, fill(pbL - outW, aa));
        col = mix(col, fur,  fill(pbL, aa));
        col = mix(col, mix(fur,outC,0.38), fill(toeBL,aa)*fill(pbL,aa));
        col = mix(col, outC, fill(pbR - outW, aa));
        col = mix(col, fur,  fill(pbR, aa));
        col = mix(col, mix(fur,outC,0.38), fill(toeBR,aa)*fill(pbR,aa));
        col = mix(col, outC, fill(neck_sdf - outW, aa));
        col = mix(col, fur,  fill(neck_sdf, aa));
        col = mix(col, outC, fill(body_sdf - outW, aa));
        col = mix(col, fur,  fill(body_sdf, aa));
        col = mix(col, mix(fur,vec3(1.0),0.42), fill(belly,aa)*fill(body_sdf,aa));
    }

    // Ears behind head
    col = mix(col, outC, fill(earL - outW, aa));
    col = mix(col, ear_color.rgb,        fill(earL, aa));
    col = mix(col, ear_color.rgb * 0.78, fill(innerEarL, aa));
    col = mix(col, outC, fill(earR - outW, aa));
    col = mix(col, ear_color.rgb,        fill(earR, aa));
    col = mix(col, ear_color.rgb * 0.78, fill(innerEarR, aa));

    // Head + muzzle
    col = mix(col, outC, fill(headShape - outW, aa));
    col = mix(col, fur,  fill(headShape, aa));
    col = mix(col, mix(fur,vec3(1.0),0.32), fill(muzzle,aa)*fill(headShape,aa));
    // Nasal bridge shadow
    col = mix(col, outC*0.55+fur*0.45, fill(nasalRidge-0.002,aa)*fill(headShape,aa));
    // Topknot curl on crown
    col = mix(col, outC, fill(topknot - outW, aa));
    col = mix(col, ear_color.rgb, fill(topknot, aa));

    // Eyes
    col = mix(col, eye_color.rgb, fill(eyeL,  aa));
    col = mix(col, eye_color.rgb, fill(eyeR2, aa));
    col = mix(col, vec3(0.05,0.03,0.01), fill(pupilL, aa)*fill(eyeL, aa)*blink);
    col = mix(col, vec3(0.05,0.03,0.01), fill(pupilR2,aa)*fill(eyeR2,aa)*blink);
    col = mix(col, vec3(1.0), fill(shineL,aa)*fill(eyeL, aa)*blink);
    col = mix(col, vec3(1.0), fill(shineR,aa)*fill(eyeR2,aa)*blink);
    col = mix(col, fur, fill(lidL, aa)*fill(eyeL, aa));
    col = mix(col, fur, fill(lidR2,aa)*fill(eyeR2,aa));
    col = mix(col, fur*0.52, fill(browL-0.001,aa)*fill(headShape,aa));
    col = mix(col, fur*0.52, fill(browR-0.001,aa)*fill(headShape,aa));

    // Nose
    col = mix(col, vec3(0.08,0.04,0.03), fill(nose, aa));
    col = mix(col, vec3(0.52,0.36,0.32), fill(noseShine,aa)*fill(nose,aa));

    // Mouth + tongue
    col = mix(col, vec3(0.08,0.03,0.05), fill(mInterior,aa)*fill(headShape,aa));
    col = mix(col, vec3(0.91,0.44,0.50), fill(tongue,   aa)*fill(headShape,aa));
    col = mix(col, vec3(0.20,0.07,0.09), fill(mouth-0.0052,aa)*fill(headShape,aa));

    // Bone over head, under front paws
    if (show_bone) {
        col = mix(col, outC, fill(bone_sdf - outW, aa));
        col = mix(col, bone_color.rgb, fill(bone_sdf, aa));
        col = mix(col, vec3(1.0), fill(sdBox(boneRel, vec2(boneHL*0.65, shaftH*0.28))-0.004, aa)*fill(bone_sdf,aa));
    }

    // Front paws on top
    if (show_body) {
        col = mix(col, outC, fill(pfL - outW, aa));
        col = mix(col, fur,  fill(pfL, aa));
        col = mix(col, mix(fur,outC,0.38), fill(toeL,aa)*fill(pfL,aa));
        col = mix(col, outC, fill(pfR - outW, aa));
        col = mix(col, fur,  fill(pfR, aa));
        col = mix(col, mix(fur,outC,0.38), fill(toeR,aa)*fill(pfR,aa));
    }

    gl_FragColor = vec4(col, 1.0);
}
