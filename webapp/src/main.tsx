import React from "react";
import { createRoot } from "react-dom/client";
import { LoginForm } from "./features/auth/LoginForm";

const App = () => (
  <>
    <h1>ProvaVida WebApp - Sprint 5</h1>
    <LoginForm />
  </>
);

const root = createRoot(document.getElementById("root")!);
root.render(<App />);
