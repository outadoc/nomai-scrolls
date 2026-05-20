import argparse
import functools
import http.server
from pathlib import Path

from nomai_web import site


def _add_common_args(parser: argparse.ArgumentParser) -> None:
    parser.add_argument(
        "--db",
        default="nomai.db",
        metavar="PATH",
        help="SQLite database to read from",
    )
    parser.add_argument(
        "--out",
        default="out",
        metavar="DIR",
        help="output directory for generated HTML",
    )
    parser.add_argument(
        "--lang",
        default="en",
        metavar="CODE",
        help="language code for translations",
    )


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate and serve a static Reddit-like HTML site from Nomai dialogue data.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    subparsers = parser.add_subparsers(dest="command", required=True)

    gen_parser = subparsers.add_parser(
        "generate",
        help="generate the static HTML site",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    _add_common_args(gen_parser)

    serve_parser = subparsers.add_parser(
        "serve",
        help="generate the site then start a local development server",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    _add_common_args(serve_parser)
    serve_parser.add_argument(
        "--port",
        default=8000,
        type=int,
        metavar="PORT",
        help="port to listen on",
    )

    args = parser.parse_args()

    site.generate(db_path=Path(args.db), out_dir=Path(args.out), lang=args.lang)

    if args.command == "serve":
        handler = functools.partial(
            http.server.SimpleHTTPRequestHandler,
            directory=str(Path(args.out).resolve()),
        )
        server = http.server.HTTPServer(("", args.port), handler)
        print(f"\nServing at http://localhost:{args.port}/")
        print("Press Ctrl+C to stop.")
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            print()
