import React from "react";

const SVG = ({
  style = {},
  fill = "#9B9B9B",
  width="24px",
  height="24px",
  viewBox="0 0 19 24",
  className = ""
}) => (
  <svg
    width={width}
    style={style}
    height={width}
    viewBox={viewBox}
    xmlns="http://www.w3.org/2000/svg"
    className={`svg-icon ${className || ""}`}
    xmlnsXlink="http://www.w3.org/1999/xlink"
  >
  <g id="Symbols" stroke="none" strokeWidth="1" fill="none" fillRule="evenodd">
      <g id="Side-menu-/-dashboard" transform="translate(-22.000000, -320.000000)">
          <g id="Group-17" transform="translate(20.000000, 321.000000)">
              <g id="task-checklist">
                  <g id="Outline_Icons" transform="translate(2.000000, 0.000000)" stroke={fill} strokeLinecap="round" strokeLinejoin="round">
                      <g id="Group">
                          <polyline id="Shape" points="16.5 2.29166667 18.5 2.29166667 18.5 21.5416667 0.5 21.5416667 0.5 2.29166667 2.5 2.29166667"></polyline>
                          <path d="M11.5,2.29166667 C11.5,1.27966667 10.605,0.458333333 9.5,0.458333333 C8.396,0.458333333 7.5,1.27966667 7.5,2.29166667 L4.5,2.29166667 L4.5,4.125 L14.5,4.125 L14.5,2.29166667 L11.5,2.29166667 Z" id="Shape"></path>
                          <polyline id="Shape" points="16.5 4.125 16.5 19.7083333 2.5 19.7083333 2.5 4.125"></polyline>
                          <path d="M7.5,7.79166667 L13.5,7.79166667" id="Shape"></path>
                          <path d="M7.5,10.5416667 L13.5,10.5416667" id="Shape"></path>
                          <path d="M7.5,13.2916667 L13.5,13.2916667" id="Shape"></path>
                          <path d="M7.5,16.0416667 L13.5,16.0416667" id="Shape"></path>
                          <path d="M5,7.79166667 L5.5,7.79166667" id="Shape"></path>
                          <path d="M5,10.5416667 L5.5,10.5416667" id="Shape"></path>
                          <path d="M5,13.2916667 L5.5,13.2916667" id="Shape"></path>
                          <path d="M5,16.0416667 L5.5,16.0416667" id="Shape"></path>
                      </g>
                  </g>
                  <g id="Invisible_Shape">
                      <rect id="Rectangle-path" x="0" y="0" width="24" height="22"></rect>
                  </g>
              </g>
          </g>
      </g>
  </g>
  </svg>
);

export default SVG;
