# Frontend Dockerfile (serve static assets with Nginx)
FROM nginx:alpine

# Clean default nginx web root
RUN rm -rf /usr/share/nginx/html/*

# Copy static assets
COPY wwwroot/ /usr/share/nginx/html/

# Copy HTML pages exported from MVC Views
COPY Views/ /usr/share/nginx/html/Views/

# Set default landing page
COPY Views/Home/Index.html /usr/share/nginx/html/index.html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
