import React from "react";

export const Button: React.FC<React.ButtonHTMLAttributes<HTMLButtonElement>> = (props) => (
  <button {...props} style={{ padding: 8, borderRadius: 4, background: '#1976d2', color: '#fff', border: 'none' }}>
    {props.children}
  </button>
);
