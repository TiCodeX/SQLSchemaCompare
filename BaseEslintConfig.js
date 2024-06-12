module.exports = {
  getBaseConfig: (dir) => {
    const eslint = require(dir + "/node_modules/@eslint/js");
    const tseslint = require(dir + "/node_modules/typescript-eslint/dist");
    const pluginSonarJS = require(dir + "/node_modules/eslint-plugin-sonarjs");
    const pluginUnicorn = require(dir + "/node_modules/eslint-plugin-unicorn");
    const pluginJsDoc = require(dir + "/node_modules/eslint-plugin-jsdoc");
    const pluginOnlyError = require(dir + "/node_modules/eslint-plugin-only-error");

    return {
      files: ["**/*.ts", "**/*.tsx"],
      extends: [
        eslint.configs.recommended,
        ...tseslint.configs.recommended,
        ...tseslint.configs.recommendedTypeChecked,
        pluginSonarJS.configs.recommended,
        pluginUnicorn.configs["flat/recommended"],
        pluginJsDoc.configs["flat/recommended-typescript"],
      ],
      plugins: {
        pluginSonarJS,
        pluginUnicorn,
        pluginJsDoc,
        pluginOnlyError,
      },
      languageOptions: {
        parser: tseslint.parser,
        parserOptions: {
          project: true,
          tsconfigRootDir: __dirname,
        },
      },
      rules: {
        // To keep disabled?
        "unicorn/throw-new-error": "off",
        "unicorn/prefer-module": "off",
        "unicorn/switch-case-braces": "off",
        "sonarjs/no-duplicate-string": "off",
        "unicorn/prevent-abbreviations": "off",
        "unicorn/no-array-for-each": "off",
      }
    };
  }
};
