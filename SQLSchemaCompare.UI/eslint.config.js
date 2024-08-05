const tseslint = require("typescript-eslint");
const baseConfig = require("../BaseEslintConfig.js").getBaseConfig(__dirname);

module.exports = tseslint.config(
  {
    ignores: [
      "**/wwwroot/**/*.js",
      "**/wwwroot/lib",
    ],
  },
  baseConfig,
);
