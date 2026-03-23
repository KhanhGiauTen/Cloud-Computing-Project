# Frontend Dockerfile (serve static assets with Nginx)
FROM nginx:alpine

# Clean default nginx web root
RUN rm -rf /usr/share/nginx/html/*

# Copy static assets (css/js/lib/favicon)
COPY wwwroot/ /usr/share/nginx/html/

# Copy exported HTML pages to web root so routes like
# /Home/Index.html, /Customers/Index.html work correctly
COPY Views/ /usr/share/nginx/html/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
