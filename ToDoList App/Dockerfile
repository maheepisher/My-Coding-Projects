FROM node:19.7.0-alpine


ENV NODE_ENV production
RUN mkdir /labone/ && chown node:node /labone/
WORKDIR /labone/

USER node

COPY --chown=node:node . .

RUN npm install



EXPOSE 3000
CMD node src/index.js
