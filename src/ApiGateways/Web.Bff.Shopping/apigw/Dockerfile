FROM nginx
COPY src/ApiGateways/Web.Bff.Shopping/apigw/nginx.conf /etc/nginx/nginx.conf
RUN rm -rf /etc/nginx/conf.d/*
COPY src/ApiGateways/Web.Bff.Shopping/apigw/services/* /etc/nginx/conf.d/
EXPOSE 80
