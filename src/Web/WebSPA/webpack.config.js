const path = require("path");
const webpack = require("webpack");
const fableUtils = require("fable-utils");

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

var babelOptions = fableUtils.resolveBabelOptions({
  presets: [
    ["env", {
      "targets": {
        "browsers": ["last 2 versions"]
      },
      "modules": false
    }]
  ],
  plugins: ["transform-runtime"]
});


const isProduction = process.argv.indexOf("-p") >= 0;
const port = process.env.WEBSPA_PORT || "8080";
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

module.exports = {
  devtool: "source-map",
  entry: resolve('./WebSPA.fsproj'),
  mode: isProduction ? "production" : "development",
  output: {
    path: resolve('./public'),
    publicPath: "/public",
    filename: "bundle.js"
  },
  resolve: {
    modules: [ resolve("../../../node_modules/")]
  },
  devServer: {
    hot: true,
    inline: true,
    historyApiFallback: true,
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      }
    ]
  },
  plugins : isProduction ? [] : [
      new webpack.HotModuleReplacementPlugin(),
      new webpack.NamedModulesPlugin()
  ]
};