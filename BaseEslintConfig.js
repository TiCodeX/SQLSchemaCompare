module.exports = {
  getBaseConfig: (dir) => {
    const eslint = require(dir + "/node_modules/@eslint/js");
    const tseslint = require(dir + "/node_modules/typescript-eslint/dist");
    const pluginOnlyError = require(dir + "/node_modules/eslint-plugin-only-error");
    const pluginSonarJS = require(dir + "/node_modules/eslint-plugin-sonarjs");
    const pluginUnicorn = require(dir + "/node_modules/eslint-plugin-unicorn");

    return {
      files: ["**/*.ts"],
      extends: [
        eslint.configs.recommended,
        ...tseslint.configs.recommended,
        ...tseslint.configs.recommendedTypeChecked,
        pluginSonarJS.configs.recommended,
        pluginUnicorn.configs["flat/recommended"],
      ],
      plugins: {
        pluginOnlyError,
        pluginSonarJS,
        pluginUnicorn,
      },
      languageOptions: {
        parser: tseslint.parser,
        parserOptions: {
          project: true,
          tsconfigRootDir: dir,
        },
      },
      rules: {
        "@typescript-eslint/no-require-imports": "off",
        "sonarjs/sonar-no-fallthrough": "off", // Rule crashes on eslint 9.x
        "unicorn/prefer-module": "off", // Electron uses CommonJS
      },
    };
  },
};
