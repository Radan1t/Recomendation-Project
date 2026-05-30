# ==========================
# Stage 1 - Build Angular
# ==========================
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

RUN npm run build --configuration=production

# ==========================
# Stage 2 - Nginx
# ==========================
FROM nginx:alpine

# Видаляємо стандартні файли nginx
RUN rm -rf /usr/share/nginx/html/*

# Копіюємо результат збірки Angular
COPY --from=build /app/dist/web-client/browser /usr/share/nginx/html

# Конфіг nginx для Angular SPA
RUN printf 'server {\n\
    listen 80;\n\
    server_name localhost;\n\
\n\
    root /usr/share/nginx/html;\n\
    index index.html;\n\
\n\
    location / {\n\
        try_files $uri $uri/ /index.html;\n\
    }\n\
}\n' > /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]