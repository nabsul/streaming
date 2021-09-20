const { Transform } = require('stream')
const JSONStream  = require('JSONStream')

module.exports.createJsonTransform = () => {
    let first = true
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            let prefix = first ? '[\n\t' : ',\n\t'
            first = false
            this.push(prefix + JSON.stringify(obj))
            cb()
        },
        final(cb) {
            this.push('\n]')
            cb()
        }
    })
}

module.exports.createSummaryTransform = () => {
    let currentDay = -1
    let totalScores = new Map()
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            const day = obj.time.substr(0, obj.time.indexOf('T'))

            // flush outputs if necessary
            if (day !== currentDay) {
                currentDay = day

                if (totalScores.size > 0) {
                    let winner = null
                    let maxScore = -1
                    for (const entry of totalScores.entries()) {
                        const [user, score] = entry
                        if (score < maxScore) continue
                        maxScore = score
                        winner = user
                    }

                    this.push({day: currentDay, winner: winner, totalScore: maxScore})
                    totalScores = new Map()
                }
            }

            const score = (totalScores.get(obj.userId) || 0) + obj.points
            totalScores.set(obj.userId, score)
            cb()
        }
    })
}

module.exports.createCsvTransform = () => {
    return new Transform({
        objectMode: true,
        transform(obj, enc, cb) {
            cb(null, `${obj.time},${obj.userId},${obj.points}\n`)
        }
    })
}

module.exports.createRawDataTransform = () => JSONStream.parse('*')
