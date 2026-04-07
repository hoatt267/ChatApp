export const APP_CONFIG = {
  API_URL: import.meta.env.VITE_API_URL || "http://localhost:5000/api",
  SIGNALR_URL:
    import.meta.env.VITE_SIGNALR_URL || "http://localhost:5000/chatHub",

  MAX_FILE_SIZE_MB: 25,
  PAGINATION_LIMIT: 20,
};
