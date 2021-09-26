use std::env;

mod generate;

fn main() {
    let args: Vec<String> = env::args().collect();
    if args.len() > 1 && args[1] == "generate" {
        return generate::generate();
    }

    server();
}

fn server() {
    println!("Runnning the server")
}
