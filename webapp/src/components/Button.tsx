import React from "react";

const defaultStyle: React.CSSProperties = {
  padding: '0.75rem 1.5rem',
  borderRadius: '8px',
  background: '#4f46e5',
  color: '#fff',
  border: 'none',
  fontSize: '1rem',
  fontWeight: '600',
  cursor: 'pointer',
  transition: 'all 0.2s',
};

export const Button: React.FC<React.ButtonHTMLAttributes<HTMLButtonElement>> = (props) => (
  <button 
    {...props} 
    style={{ 
      ...defaultStyle, 
      ...props.style,
      opacity: props.disabled ? 0.6 : 1,
      cursor: props.disabled ? 'not-allowed' : 'pointer',
    }}
    disabled={props.disabled}
  >
    {props.children}
  </button>
);
