module.exports = {
    root: true,
    env: {
        es6: true,
        node: true,
        browser: true,
    },
    parserOptions: {
        project: "./tsconfig.eslint.json",
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        tsconfigRootDir: __dirname,
    },
    plugins: ["only-error"],
    extends: ["@neolution-ch/eslint-config-neolution"],
    rules: {
        // Need to be changed in .editorconfig
        "@typescript-eslint/indent": "off",
        "linebreak-style": ["error", "windows"],

        // TODO:
        "@typescript-eslint/naming-convention": "off",
        "@typescript-eslint/lines-around-comment": "off",
        "@typescript-eslint/no-unused-vars": "off",
        "max-lines": "off",
    },
};
