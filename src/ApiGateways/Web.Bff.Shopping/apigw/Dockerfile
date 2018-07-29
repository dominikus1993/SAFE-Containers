FROM nginx
COPY src/nginx/nginx.conf /etc/nginx/nginx.conf
RUN rm -rf /etc/nginx/conf.d/*
COPY src/nginx/services/* /etc/nginx/conf.d/
EXPOSE 80
