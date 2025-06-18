import React from 'react';

const CIRCLE_RADIUS = 20;
const CIRCLE_CIRCUMFERENCE = 2 * Math.PI * CIRCLE_RADIUS;

export default function ModuleProgress({ current, total }) {
  const progress = total > 0 ? current / total : 0;
  const dashoffset = CIRCLE_CIRCUMFERENCE * (1 - progress);

  return (
    <div className="module-progress">
      <svg width="44" height="44">
        <circle
          cx="22"
          cy="22"
          r={CIRCLE_RADIUS}
          stroke="#e6e6e6"
          strokeWidth="4"
          fill="none"
        />
        <circle
          cx="22"
          cy="22"
          r={CIRCLE_RADIUS}
          stroke="#8d5cf6"
          strokeWidth="4"
          fill="none"
          strokeDasharray={CIRCLE_CIRCUMFERENCE}
          strokeDashoffset={dashoffset}
          strokeLinecap="round"
        />
        <text
          x="50%"
          y="54%"
          textAnchor="middle"
          fill="#8d5cf6"
          fontSize="14"
          fontWeight="700"
          dy="0.3em"
        >
          {current}/{total}
        </text>
      </svg>
    </div>
  );
}
