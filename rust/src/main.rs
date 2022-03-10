use std::env;

mod generate;
mod server;

fn main() -> std::io::Result<()> {
    let args: Vec<String> = env::args().collect();
    if args.len() > 1 && args[1] == "generate" {
        generate::generate();
        return Ok(());
    }

    return server::serve();
}
