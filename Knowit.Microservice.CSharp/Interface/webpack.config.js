const path = require("path");
const CopyWebpackPlugin = require('copy-webpack-plugin');
const HtmlWebpackPlugin = require("html-webpack-plugin");
const HtmlWebpackTagsPlugin = require('html-webpack-tags-plugin');

const webpackOptions = {
  mode: "development",
  resolve: {
    extensions: [".ts", ".tsx", ".js", ".jsx"]
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/, 
        loaders: [
          {loader: "babel-loader", options: {presets: ["@babel/preset-env"]}},
          {loader: "ts-loader"}
        ]
      }
    ]
  },
  devtool: "source-map"
};

const app = {
  ...webpackOptions,
  entry: "./Components/App.tsx",
  output: {
    filename: "app.js",
    path: path.resolve("../Host/wwwroot"),
    library: "ProjectName",
    libraryTarget: "umd"
  },
  externals: {"react": "React"},
};

const copyPluginOptions = [
  {from: "node_modules/react/umd/react.development.js", to: "react.js"},
  {from: "node_modules/react-dom/umd/react-dom.development.js", to: "react-dom.js"}
];

const htmlPluginOptions = {
  title: "ProjectName"
};

const htmlTagsPluginOptions = {
  scripts: [
    {path: "react.js", external: {packageName: "react", variableName: "React"}},
    {path: "react-dom.js", external: {packageName: "react-dom", variableName: "ReactDOM"}},
    {path: "app.js", external: {packageName: "Components/App", variableName: "ProjectName"}},
  ]
};

const index = {
  ...webpackOptions,
  entry: "./index.tsx",
  output: {
    filename: "main.js",
    path: path.resolve("../Host/wwwroot")
  },
  plugins: [
    new CopyWebpackPlugin(copyPluginOptions),
    new HtmlWebpackPlugin(htmlPluginOptions),
    new HtmlWebpackTagsPlugin(htmlTagsPluginOptions)
  ]
};

module.exports = [app, index];
