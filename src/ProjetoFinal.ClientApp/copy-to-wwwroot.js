// copy-to-wwwroot.js
const {execSync} = require("child_process");
const os = require("os");


try {
  if (IsDevelopment()) {
    const command = getCommandByEnvironmentAndOs();
    console.info(`💻 Comando a ser executado: ${command}`);
    execSync(command, {stdio: "ignore"});
  }
} catch (error) {
  console.error("❌ Erro ao copiar arquivos:", error.message);
  process.exit(0);
}

function getCommandByEnvironmentAndOs() {
  const isWindows = os.platform() === "win32";
  const {from, to} = getPaths(isWindows);
  let completeCommandLinux = `rm -rf ${to}/* && cp -r ${from}/* ${to}/`;
  let completeCommandWindows = `xcopy /E /Y /I ${from} ${to}`;
  if (isWindows)
    return completeCommandWindows;
  return completeCommandLinux;
}

function getPaths(isWindows) {
  let from = "dist/browser";
  let to = IsDevelopment() ? "../ProjetoFinal.Api/wwwroot" : "wwwroot";
  if (isWindows) {
    from = from.replaceAll(/\//g, "\\");
    to = to.replaceAll(/\//g, "\\");
  }
  return {from, to};
}

function IsDevelopment() {
  const ambiente = process.env.ASPNETCORE_ENVIRONMENT ?? "Development";
  console.log(`Environment: ${ambiente}`);
  return ambiente === "Development";
}
