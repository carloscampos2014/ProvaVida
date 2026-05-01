import axios from "axios";

const baseURL = import.meta.env.VITE_API_URL || "http://localhost:58271";

const api = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor para tratar erros
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response) {
      // Erro retornado pelo servidor
      const mensagem = error.response.data?.mensagem || "Erro no servidor";
      return Promise.reject(new Error(mensagem));
    } else if (error.request) {
      // Servidor não respondeu
      return Promise.reject(new Error("API indisponível. Verifique se o backend está rodando."));
    }
    return Promise.reject(error);
  }
);

export default api;
